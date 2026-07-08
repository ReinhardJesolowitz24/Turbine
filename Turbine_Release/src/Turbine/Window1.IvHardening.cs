// =====================================================================
//  IV-Haertung (2026-07-08)
//  --------------------------------------------------------------------
//  Erzeugt das 16-Byte-IV aus ZWEI unabhaengigen Quellen und verdichtet
//  sie per SHA-256 (Randomness-Extractor):
//    Quelle 1: OS-CSPRNG  (BCryptGenRandom via RNGCryptoServiceProvider)
//    Quelle 2: Timing-Jitter + Umgebung  (UNABHAENGIG vom CNG-Pool)
//
//  Zweck (Defense-in-Depth): Waere der OS-CSPRNG manipuliert/vorhersehbar
//  (historisches Beispiel: Dual_EC_DRBG), bliebe das IV dennoch
//  unvorhersehbar, solange die unabhaengige Zweitquelle beitraegt. Die
//  Zweitquelle ist bewusst "low-tech" (Timing-Jitter): schwach in der
//  Entropiemenge, aber klar unabhaengig und ohne native Interop.
//
//  Format-transparent: KEIN neues Versions-/Varianten-Byte noetig. Nur die
//  Erzeugung der 16 IV-Bytes aendert sich; das IV wird wie bisher in die
//  Datei geschrieben und beim Entschluesseln zurueckgelesen. Altdateien
//  bleiben unveraendert entschluesselbar.
//
//  Untersuchung, die zu dieser Aenderung fuehrte: IV_HARDENING.md (Repo-Root/Turbine_Release)
// =====================================================================
using System;
using System.Text;
using System.Security.Cryptography;

namespace Turbine
{
    public partial class Window1
    {
        /// <summary>Gehaertete 16-Byte-IV-Erzeugung (CNG + unabhaengiger Jitter, SHA-256).</summary>
        public static void GenerateIV16(byte[] dest16)
        {
            byte[] cng = new byte[32];
            using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(cng); }   // Quelle 1: OS-CSPRNG
            GenerateIV16Core(cng, dest16);
        }

        /// <summary>Kombiniert die (uebergebene) CNG-Quelle mit unabhaengigem Jitter per SHA-256.</summary>
        private static void GenerateIV16Core(byte[] cng, byte[] dest16)
        {
            byte[] jitter = GatherJitterEntropy();                                    // Quelle 2: unabhaengig
            using (var sha = SHA256.Create())
            {
                byte[] buf = new byte[cng.Length + jitter.Length];
                Buffer.BlockCopy(cng, 0, buf, 0, cng.Length);
                Buffer.BlockCopy(jitter, 0, buf, cng.Length, jitter.Length);
                byte[] h = sha.ComputeHash(buf);                                      // Randomness-Extractor
                Array.Copy(h, 0, dest16, 0, 16);
            }
        }

        /// <summary>
        /// Unabhaengiger Entropiepool aus Timing-Jitter (NICHT aus dem CNG-Pool) + Umgebung.
        /// Die Entropie steckt in den schwankenden Laufzeiten (now-prev), nicht im deterministischen acc.
        /// Bewusst KEIN Guid.NewGuid() als Quelle - das ist unter Windows selbst CNG-basiert (nicht unabhaengig).
        /// </summary>
        private static byte[] GatherJitterEntropy()
        {
            var sb = new StringBuilder(8192);
            long prev = System.Diagnostics.Stopwatch.GetTimestamp();
            ulong acc = 0xCBF29CE484222325UL;                 // deterministische "Arbeit", deren TIMING jittert
            for (int i = 0; i < 8192; i++)
            {
                acc = acc * 1099511628211UL + (ulong)i;
                long now = System.Diagnostics.Stopwatch.GetTimestamp();
                sb.Append(now - prev).Append(':');            // das Jitter-Delta ist die Entropie
                prev = now;
            }
            sb.Append('#').Append(acc);                       // verhindert Wegoptimieren der Schleife
            sb.Append('|').Append(Environment.TickCount);
            sb.Append('|').Append(System.Diagnostics.Process.GetCurrentProcess().Id);
            sb.Append('|').Append(System.Threading.Thread.CurrentThread.ManagedThreadId);
            sb.Append('|').Append(DateTime.UtcNow.Ticks);
            sb.Append('|').Append(GC.GetTotalMemory(false));
            using (var sha = SHA256.Create())
                return sha.ComputeHash(Encoding.ASCII.GetBytes(sb.ToString()));       // 32 B
        }
    }
}
