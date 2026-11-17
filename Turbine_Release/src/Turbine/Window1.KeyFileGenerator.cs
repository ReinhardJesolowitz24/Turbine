// =====================================================================
//  Key-File-Generator (V6.2)
//  --------------------------------------------------------------------
//  Ersetzt den frueheren Knopf "wipe Source file". Mehrfaches Ueberschreiben
//  loescht auf SSDs / USB-Sticks nicht zuverlaessig (Wear-Leveling: der
//  Controller schreibt an neue Zellen, die alten bleiben unsichtbar liegen).
//
//  Erzeugt eine neue Key-Datei (8 KB) aus ZWEI unabhaengigen Quellen, wie
//  beim IV (Window1.IvHardening.cs):
//    Quelle 1: OS-CSPRNG (RNGCryptoServiceProvider / BCryptGenRandom), 8 KB
//    Quelle 2: Timing-Jitter (GatherJitterEntropy, 2 x 32 B) -> SHA-512-
//              Zaehlerstrom ueber "TURBINE-KEYGEN-v1" || seed || ctr
//    Ausgabe  = Quelle 1 XOR Quelle 2
//  Ist eine Quelle gut, ist die Ausgabe nicht vorhersagbar.
//
//  Kein neues Dateiformat: die erzeugte Datei ist ein gewoehnliches Key-File
//  (> 7000 B), nutzbar fuer 0x09 (auch in V6.0) und 0x0A.
//  Eine vorhandene Datei wird NIE ueberschrieben (koennte ein benutztes
//  Key-File sein -> Datenverlust).
// =====================================================================
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace Turbine
{
    public partial class Window1
    {
        public const int GENERATED_KEYFILE_SIZE = 8192;

        /// <summary>Neue Key-File-Bytes: OS-CSPRNG XOR SHA-512-Strom aus unabhaengigem Jitter.</summary>
        public static byte[] GenerateKeyFileBytes(int size)
        {
            byte[] cng = new byte[size];
            using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(cng); }   // Quelle 1
            byte[] result = GenerateKeyFileBytesCore(cng);
            Array.Clear(cng, 0, cng.Length);
            return result;
        }

        /// <summary>Mischt die uebergebene CNG-Quelle mit unabhaengigem Jitter (testbar mit kaputter Quelle 1).</summary>
        internal static byte[] GenerateKeyFileBytesCore(byte[] cng)
        {
            byte[] label = Encoding.ASCII.GetBytes("TURBINE-KEYGEN-v1");
            byte[] blk = new byte[label.Length + 64 + 4];
            Buffer.BlockCopy(label, 0, blk, 0, label.Length);
            Buffer.BlockCopy(GatherJitterEntropy(), 0, blk, label.Length, 32);         // Quelle 2
            Buffer.BlockCopy(GatherJitterEntropy(), 0, blk, label.Length + 32, 32);
            byte[] output = new byte[cng.Length];
            using (var sha = SHA512.Create())
            {
                int pos = 0;
                uint ctr = 0;
                int c = blk.Length - 4;
                while (pos < cng.Length)
                {
                    blk[c] = (byte)(ctr >> 24); blk[c + 1] = (byte)(ctr >> 16);
                    blk[c + 2] = (byte)(ctr >> 8); blk[c + 3] = (byte)ctr;
                    ctr++;
                    byte[] h = sha.ComputeHash(blk);
                    for (int i = 0; i < h.Length && pos < cng.Length; i++, pos++)
                        output[pos] = (byte)(cng[pos] ^ h[i]);
                    Array.Clear(h, 0, h.Length);
                }
            }
            Array.Clear(blk, 0, blk.Length);
            return output;
        }

        /// <summary>Knopf "generate key file".</summary>
        private void GenerateKeyFile_Click(object sender, RoutedEventArgs e)
        {
            if (prozess_laueft) return;

            MessageBoxResult intro = MessageBox.Show(
                "This creates a new key file: " + GENERATED_KEYFILE_SIZE + " bytes of random data from two " +
                "independent sources (Windows random generator and CPU timing jitter).\n\n" +
                "IMPORTANT: Files encrypted with this key file can ONLY be decrypted with it. " +
                "If you lose it, the data is lost.\n" +
                "Choose an inconspicuous name and store the key file apart from the encrypted files.\n\n" +
                "Create a key file now?",
                "Generate key file", MessageBoxButton.YesNo);
            if (intro != MessageBoxResult.Yes) return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save the new key file";
            dlg.Filter = "Binary file (*.bin)|*.bin|All files (*.*)|*.*";
            dlg.DefaultExt = ".bin";
            dlg.AddExtension = true;
            dlg.OverwritePrompt = false;      // eigene, strengere Pruefung unten
            if (dlg.ShowDialog() != true) return;
            string path = dlg.FileName;

            if (File.Exists(path))
            {
                ShowFg("This file already exists and is NOT overwritten - it might be a key file " +
                       "that is still needed. Please choose a new file name.");
                return;
            }

            byte[] key = null;
            try
            {
                key = GenerateKeyFileBytes(GENERATED_KEYFILE_SIZE);
                if (!KeyMaterialEntropyOK(key, key.Length))
                {
                    ShowFg("The generated data failed the randomness check. Nothing was written - please try again.");
                    return;
                }
                using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                                               4096, FileOptions.WriteThrough))
                {
                    fs.Write(key, 0, key.Length);
                    fs.Flush(true);
                }
                byte[] back = File.ReadAllBytes(path);
                bool same = back.Length == key.Length;
                for (int i = 0; same && i < key.Length; i++) same = back[i] == key[i];
                Array.Clear(back, 0, back.Length);
                if (!same)
                {
                    try { File.Delete(path); } catch { }
                    ShowFg("The key file could not be verified after writing and was removed. " +
                           "Please try again or choose another drive.");
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowFg("The key file could not be written: " + ex.Message);
                return;
            }
            finally
            {
                if (key != null) Array.Clear(key, 0, key.Length);
            }

            MessageBoxResult load = MessageBox.Show(
                "Key file created:\n" + path + "\n(" + GENERATED_KEYFILE_SIZE + " bytes)\n\n" +
                "Make a copy on a separate medium NOW (e.g. a second USB stick). " +
                "Without this file, data encrypted with it cannot be recovered.\n\n" +
                "Load it as key file now?",
                "Key file created", MessageBoxButton.YesNo);
            if (load == MessageBoxResult.Yes)
            {
                ResetKeyFileUi();
                LoadKeyFileFromPath(path);
            }
        }
    }
}
