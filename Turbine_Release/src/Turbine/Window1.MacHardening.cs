// =====================================================================
//  MAC-Haertung (2026-09) -- Encrypt-then-MAC mit HMAC-SHA-384
//  --------------------------------------------------------------------
//  Neue Versionsbytes: 0x08 = Passwort + MAC, 0x09 = Key-File + MAC.
//  Die Chiffrierung ist identisch zu 0x06/0x07 (nur die Versions-Conditionals
//  in DoWork erkennen 0x08/0x09 zusaetzlich). Zusaetzlich wird ein 48-Byte-Tag
//  angehaengt.
//
//  Bauart (Encrypt-then-MAC, die kryptografisch korrekte Reihenfolge):
//    Tag = HMAC-SHA384(K_mac, gesamte fertige Datei[Header+IV+Chiffretext]), 48 B, ans Ende.
//  Entschluesselung prueft den Tag ZUERST (verify-before-decrypt); bei Mismatch
//  wird KEIN Klartext geschrieben (Fail-safe) -> loest zugleich das Problem
//  "falsches Passwort/Key meldet Erfolg zu Muell".
//
//  Schluesseltrennung: K_mac wird domaenensepariert AUS dem bereits (langsam per
//  PBKDF2 bzw. SHA-512-Whitening) abgeleiteten Master-Key gewonnen
//  (name_der_datei6 = KDF-Expansion). Dadurch liegt K_mac HINTER der langsamen KDF
//  -> der MAC ist KEIN schnelles Passwort-Orakel.
//
//  Deckt ab: Versionsbyte + IV + Chiffretext (alles ausser dem Tag selbst).
//  Rest-Punkt: Downgrade 0x08->0x06 entfernt (kann nicht faelschen) die Pruefung;
//  ein optionaler Strict-Mode (nur 0x08/0x09 akzeptieren) wuerde auch das schliessen.
// =====================================================================
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Turbine
{
    public partial class Window1
    {
        // Neue Verschluesselungen bekommen immer einen MAC (Versionsbyte 0x08/0x09).
        public static bool MacMode = true;

        // Von DoWork gesetzt, wenn die MAC-Verifikation beim Entschluesseln fehlschlaegt.
        private bool MacFailed = false;

        // Von DoWork gesetzt, wenn beim VERSCHLUESSELN ein entartetes Key-File abgelehnt wird (C1).
        private bool LowEntropyKeyReject = false;

        private const int MAC_TAG_LEN = 48;   // HMAC-SHA-384 = 48 Byte

        /// K_mac aus dem expandierten Master-Key (name_der_datei6, nach der langsamen KDF).
        private byte[] DeriveMacKey()
        {
            byte[] km = name_der_datei6 ?? new byte[0];
            using (var h = new HMACSHA384(km))
                return h.ComputeHash(Encoding.ASCII.GetBytes("TURBINE-MAC-v1"));
        }

        private static bool MacConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        // ===== C1-Schutz: Key-File-Entropie =====
        // Lehnt entartete Key-Files ab (einfarbige/synthetische Bilder, null-gepaddete/
        // stille Dateien), bei denen die drei XOR-Bereiche zu einem quasi-konstanten
        // Schluessel zusammenfallen. Ablehnen bei < 64 verschiedenen Byte-Werten ODER
        // < 6,0 bit/Byte Shannon-Entropie. Echte Fotos (v.a. JPEG) liegen bei ~256
        // Werten / ~7,9 bit und passieren problemlos.
        public static bool KeyMaterialEntropyOK(byte[] data, int len)
        {
            if (data == null || len <= 0) return false;
            int[] freq = new int[256];
            for (int i = 0; i < len; i++) freq[data[i]]++;
            int distinct = 0; double ent = 0.0;
            for (int v = 0; v < 256; v++)
                if (freq[v] > 0)
                {
                    distinct++;
                    double p = (double)freq[v] / len;
                    ent -= p * Math.Log(p, 2.0);
                }
            return distinct >= 64 && ent >= 6.0;
        }

        /// Encrypt-then-MAC: HMAC-SHA-384 ueber die gesamte fertige Datei, 48-B-Tag anhaengen.
        private void AppendMac(string path)
        {
            byte[] kmac = DeriveMacKey();
            byte[] all = File.ReadAllBytes(path);
            byte[] tag;
            using (var h = new HMACSHA384(kmac)) tag = h.ComputeHash(all);
            using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write)) fs.Write(tag, 0, MAC_TAG_LEN);
        }

        /// Verify-before-decrypt: Tag ueber Datei[0..len-48] neu rechnen, konstant-zeitig vergleichen.
        private bool VerifyMac(string path)
        {
            byte[] kmac = DeriveMacKey();
            byte[] all = File.ReadAllBytes(path);
            if (all.Length < 70 + MAC_TAG_LEN) return false;
            int bodyLen = all.Length - MAC_TAG_LEN;
            byte[] body = new byte[bodyLen]; Array.Copy(all, 0, body, 0, bodyLen);
            byte[] tag = new byte[MAC_TAG_LEN]; Array.Copy(all, bodyLen, tag, 0, MAC_TAG_LEN);
            byte[] calc; using (var h = new HMACSHA384(kmac)) calc = h.ComputeHash(body);
            return MacConstantTimeEquals(calc, tag);
        }
    }
}
