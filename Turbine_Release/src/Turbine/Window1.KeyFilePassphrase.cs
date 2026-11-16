// =====================================================================
//  Key-File + Passphrase (Format 0x0A) - Besitz UND Wissen.
//
//  Ist ein Key-File geladen und zusaetzlich eine Passphrase eingegeben,
//  entsteht beim Verschluesseln Format 0x0A. Das Key-File geht IN die
//  langsame KDF ein, nicht dahinter - jedes Paar (Key-File, Passphrase)
//  kostet einen vollen PBKDF2-Lauf:
//
//    kf_tag = SHA-512( "TURBINE-KFPW-v1" || kf[0..1022] )
//    pw_in  = kf_tag || UTF-8(passphrase)
//    master = PBKDF2-HMAC-SHA512( pw_in, salt = IV, 1 200 000, 64 B )
//
//  Danach wie 0x08/0x09: Counter-Mode-Expansion auf 1024 B, S-Box 0x1F3,
//  HMAC-SHA-384 (Encrypt-then-MAC). Der Cipher-Kern bleibt unveraendert.
// =====================================================================
using System;
using System.Security.Cryptography;
using System.Text;

namespace Turbine
{
    public partial class Window1
    {
        /// Passphrase im Key-File-Modus; leer = reiner Key-File-Modus (0x09).
        private string kf_passphrase = "";

        /// Von DoWork gesetzt, wenn die Eingaben vor der KDF abgelehnt werden (Meldung in RunWorkerCompleted).
        private string InputRejectMessage = null;

        private const byte FORMAT_KFPW = 0x0A;          // Key-File + Passphrase + MAC
        private const byte FORMAT_NEWEST = 0x0A;        // hoechstes bekanntes Format

        /// MAC-Formate (Tag am Dateiende, S-Box 0x1F3).
        private static bool IsMacFormat(byte v)
        {
            return v == 0x08 || v == 0x09 || v == FORMAT_KFPW;
        }

        /// KDF-Eingabe fuer 0x0A: SHA-512("TURBINE-KFPW-v1" || kf[0..1022]) || UTF-8(passphrase).
        private static byte[] BuildKfPwInput(byte[] kf, string passphrase)
        {
            byte[] label = Encoding.ASCII.GetBytes("TURBINE-KFPW-v1");
            byte[] kf_tag;
            using (var sha = SHA512.Create())
            {
                byte[] buf = new byte[label.Length + 1023];
                Array.Copy(label, 0, buf, 0, label.Length);
                Array.Copy(kf, 0, buf, label.Length, 1023);
                kf_tag = sha.ComputeHash(buf);
            }
            byte[] pp = Encoding.UTF8.GetBytes(passphrase ?? "");
            byte[] result = new byte[kf_tag.Length + pp.Length];
            Array.Copy(kf_tag, 0, result, 0, kf_tag.Length);
            Array.Copy(pp, 0, result, kf_tag.Length, pp.Length);
            return result;
        }

        /// Prueft die Eingaben VOR der langsamen KDF. null = ok, sonst Meldungstext (keine Ausgabe).
        /// Laengenregel nur beim Verschluesseln: alte Dateien mit kurzem Passwort bleiben lesbar.
        private string CheckInputsBeforeKdf(byte version_byte_of_file)
        {
            bool confirm_ok = passwort1.Equals(passwort2) || radioButton1_global;

            if (richtung_info == 0)
            {
                if (schluesseldatei_geladen == 0)
                {
                    if (passwort1.Length < 6) return "Error: The key is too short (minimum 6 characters)! Nothing was encrypted.";
                    if (passwort1.Length > 1024) return "Error: The key is too long (maximum 1024 characters)! Nothing was encrypted.";
                    if (!confirm_ok) return "Error: Keys are not equal! Nothing was encrypted.";
                }
                else if (kf_passphrase.Length > 0)
                {
                    if (kf_passphrase.Length < 6) return "Error: The passphrase for the key file is too short (minimum 6 characters)! To use the key file without a passphrase, leave the field empty. Nothing was encrypted.";
                    if (kf_passphrase.Length > 1024) return "Error: The passphrase is too long (maximum 1024 characters)! Nothing was encrypted.";
                    if (!confirm_ok) return "Error: Passphrases are not equal! Nothing was encrypted.";
                }
                return null;
            }

            // Entschluesseln
            if (version_byte_of_file > FORMAT_NEWEST)
                return "This file was created by a newer Turbine version (format 0x" + version_byte_of_file.ToString("X2")
                     + ") and cannot be opened with this version. No output was written.";
            if (version_byte_of_file == FORMAT_KFPW)
            {
                if (schluesseldatei_geladen == 0)
                    return "This file needs the key file AND the passphrase: load the key file first, then type in the passphrase. No output was written.";
                if (kf_passphrase.Length == 0)
                    return "This file needs the passphrase in addition to the key file. No output was written.";
                if (!confirm_ok)
                    return "Error: Passphrases are not equal! No output was written.";
            }
            return null;
        }

        /// Zeigt im Key-File-Modus an, ob zusaetzlich eine Passphrase verwendet wird.
        private void UpdateKeyFileModeLabel()
        {
            string pp = (passwort_anzeige == 1) ? textBox5.Text : textBox3.Password;
            bool with_pp = pp.Length > 0;
            label2.Content = with_pp ? "key file + passphrase" : "key file is loaded";
            label2.Visibility = System.Windows.Visibility.Visible;
            // Bestaetigungsfeld nur bei verdeckter Passphrase
            label4.Visibility = (with_pp && passwort_anzeige == 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        /// Uebernimmt die Eingaben aus der GUI (oder dem Test) in die Arbeitsfelder.
        /// pw  = Passwort (Passwort-Modus) bzw. optionale Passphrase (Key-File-Modus)
        /// pw2 = Bestaetigungsfeld
        private void ApplyKeyInputs(string pw, string pw2)
        {
            if (pw == null) pw = "";
            if (pw2 == null) pw2 = "";
            passwort1 = pw;
            passwort2 = pw2;
            if (schluesseldatei_geladen == 0)
            {
                kf_passphrase = "";
                name_der_datei6 = StringToByteArray(pw);
                passwortgroesse = pw.Length;
                gen_passwort = pw.Length;
            }
            else
            {
                name_der_datei6 = name_der_datei6X;
                passwortgroesse = 1023;
                gen_passwort = 1023;
                if (pw.Length == 0)
                {
                    kf_passphrase = "";
                    passwort1 = "xxxyyy";       // Platzhalter wie bisher (Pruefung in DoWork)
                    passwort2 = "xxxyyy";
                }
                else
                {
                    kf_passphrase = pw;
                }
            }
        }
    }
}
