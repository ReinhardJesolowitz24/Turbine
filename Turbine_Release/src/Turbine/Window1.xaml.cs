


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

//using System;
using System.Security.Cryptography;
//using System.Text;
//using System.IO;



//using System.Drawing;
using System.Threading;
//using System.Windows.Forms;

using System.ComponentModel;
//using System.Drawing;
using System.Net;
using System.Reflection;




namespace Turbine
{


    /// <summary>
    /// Interaktionslogik für Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        //FileStream fx;

        // ===== Turbine-S-Box (V4 Post-Whitening) =====
        // Kryptographisch aequivalent zur AES-S-Box (Nichtlinearitaet=112, Diff.Uniformitaet=4),
        // aber mit eigenem irreduziblem Polynom 0x11D und affiner Konstante 0x4A.
        // Wird auf das Keystream-Byte angewendet, um Chi²-Byte-Verteilungs-Bias zu eliminieren.
        private static readonly byte[] TURBINE_SBOX = {
            0x4A, 0xBB, 0x19, 0x28, 0xE3, 0x59, 0x7B, 0xFF, 0x35, 0x14, 0x68, 0x22, 0xD2, 0xE0, 0x90, 0x89,
            0x5E, 0xF4, 0xCE, 0xA6, 0xF0, 0xC0, 0x7E, 0x2C, 0xAD, 0x7A, 0x1F, 0xB2, 0x8C, 0xA0, 0xAB, 0x18,
            0x40, 0x49, 0x15, 0xD8, 0x08, 0x0D, 0x3C, 0xDE, 0x17, 0xD1, 0x0F, 0x5A, 0x50, 0x2F, 0x79, 0xA3,
            0xB9, 0xCC, 0x52, 0xDF, 0x4B, 0xDC, 0x36, 0x82, 0x29, 0x97, 0x94, 0xF2, 0x11, 0x02, 0x63, 0x01,
            0x4F, 0x73, 0x60, 0xE7, 0x4E, 0x9D, 0xA8, 0x8D, 0x6B, 0x3D, 0xE9, 0xFE, 0x71, 0xDD, 0x00, 0xBF,
            0xE4, 0x66, 0x87, 0xFA, 0x43, 0x39, 0x42, 0xF9, 0x47, 0x3F, 0x53, 0x1A, 0x78, 0xE1, 0xBE, 0xC7,
            0xB3, 0xFC, 0xA2, 0xB4, 0xED, 0x09, 0x80, 0x8F, 0xCA, 0x0B, 0xAA, 0x81, 0x74, 0xB8, 0x2E, 0x88,
            0xFB, 0x20, 0xA4, 0xAF, 0x8E, 0x72, 0xBD, 0x69, 0x4C, 0xC2, 0x6E, 0x0E, 0x75, 0x77, 0x44, 0xA7,
            0xC8, 0xEE, 0x7D, 0xEC, 0x5F, 0x86, 0x37, 0x93, 0x48, 0x9A, 0xA1, 0xCD, 0x3B, 0x76, 0xA9, 0xD9,
            0xDA, 0x8B, 0xF1, 0x6C, 0x30, 0xDB, 0x10, 0x61, 0xD7, 0x99, 0x2A, 0x6A, 0xC4, 0x34, 0x1B, 0x6F,
            0x1D, 0x51, 0xF7, 0xEA, 0xAC, 0x2D, 0x12, 0x7C, 0x65, 0xF6, 0xF3, 0x23, 0xE5, 0xC5, 0x38, 0x46,
            0x67, 0xC6, 0x5B, 0x95, 0x6D, 0x0C, 0xC9, 0xD6, 0xF8, 0xCF, 0x9F, 0x8A, 0x9B, 0xC1, 0x27, 0xB7,
            0xB6, 0x57, 0xBA, 0x5D, 0x3E, 0x1C, 0x9E, 0x06, 0x32, 0x3A, 0xEB, 0x45, 0x84, 0xAE, 0x03, 0x07,
            0x0A, 0xE6, 0x41, 0xCB, 0x91, 0x24, 0x04, 0xF5, 0x55, 0x05, 0x33, 0x98, 0xD3, 0x26, 0x2B, 0x5C,
            0x92, 0x31, 0xD4, 0xEF, 0x96, 0x62, 0x13, 0x1E, 0x83, 0xD0, 0xFD, 0x54, 0xB1, 0x85, 0x70, 0x64,
            0xE2, 0x25, 0xA5, 0x9C, 0x58, 0xE8, 0xC3, 0xB0, 0xD5, 0x16, 0x7F, 0x21, 0x4D, 0xB5, 0xBC, 0x56
        };

        //long gesamt = 0;
        volatile bool prozess_laueft = false;
        int fortschritt = 0;
        int fortschritt_merker = 0;
        //int test1 = 0;
        bool radioButton1_global = false;
        //bool radioButton4_global = false;
        int passwortgroesse = 0;
        long passwortende;
        string passwort1;
        string passwort2;

        string dateigroesse1;
        string dateigroesse2;

        string datei_endung1;
        string datei_endung2;
        string datei_endung_sp1;
        string datei_endung_sp2;

        int datei_endung_info1;
        int datei_endung_info2;
        int datei_endung_info3;
        int datei_endung_info4;

        byte schluesseldatei_geladen = 0;

        byte richtung_info = 0;

        bool algo = true;

        int dateil1 = 0;
        int dateil2 = 0;

        byte[] name_der_datei6 = new byte[3000];
        byte[] name_der_datei6X = new byte[3000];
        int gen_passwort = 0;

        int bildwechsel = 0;
        byte bildwechsel_merker = 0;

        byte passwort_anzeige = 0;


        byte dummy_byte1 = 0;
        byte dummy_byte2 = 0;
        byte dummy_byte3 = 0;
        byte dummy_byte4 = 0;
        byte dummy_byte5 = 0;
        byte dummy_byte6 = 0;
        byte dummy_byte7 = 0;
        byte dummy_byte8 = 0;
        byte dummy_byte9 = 0;
        byte dummy_byte10 = 0;
        byte dummy_byte11 = 0;

        byte[] Turbine_Name = new byte[7];
        byte[] Turbine_Typ_Endung = new byte[4];
        string Turbine_Header;
        string Turbine_Typ;

        /*AES*/









        /*AES_END*/




        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        public Window1()
        {
            InitializeComponent();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker2.WorkerReportsProgress = true;
            this.backgroundWorker2.WorkerSupportsCancellation = true;
            InitializeBackgoundWorker2();


            InitializeBackgoundWorker();


        }




        // Set up the BackgroundWorker object by 
        // attaching event handlers. 
        private void InitializeBackgoundWorker()
        {
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged +=
                new ProgressChangedEventHandler(
            backgroundWorker1_ProgressChanged);
        }
        private void InitializeBackgoundWorker2()
        {
            backgroundWorker2.DoWork +=
                new DoWorkEventHandler(backgroundWorker2_DoWork);
            backgroundWorker2.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            backgroundWorker2_RunWorkerCompleted);
            backgroundWorker2.ProgressChanged +=
                new ProgressChangedEventHandler(
            backgroundWorker2_ProgressChanged);
        }

        private void backgroundWorker2_DoWork(object sender2,
        DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker2 = sender2 as BackgroundWorker;
            int fortschritt = 0;
            int fortschritt_merker = 0;
            long zeichen = 0;
            long zeichen_alt = 0;
            int bildwechsel = 0;
            byte bildwechsel_merker = 0;
            byte schreib = 0;


            try
            {
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                string message = "Before deleting, the file will be overwritten ten times.\nBeware: A recovery is not possible anymore!";
                string caption = "Warning";
                string erg = "Yes";
                string erg2;


                //MessageBox.Show(message, caption, buttons);
                // Show message box
                MessageBoxResult result = MessageBox.Show(message, caption, buttons);
                erg2 = result.ToString();
                //MessageBox.Show(""+result);
                if (erg.Equals(erg2))
                {
                    zeichen_alt = 0;
                    zeichen = 0;
                    for (int durchlauf = 0; durchlauf < 2; durchlauf++)
                    {
                        schreib = (byte)(0xFF);
                        FileStream fileStr3 = new FileStream(@dateigroesse1, FileMode.Open, FileAccess.Write);
                        BinaryWriter binWriter3 = new BinaryWriter(fileStr3);
                        for (long i = 0; i < fileStr3.Length; i++)
                        {
                            //binWriter3.Write((schreib));
                            fileStr3.WriteByte(schreib);




                            zeichen++;

                            if (worker2.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }

                            if ((zeichen) > (zeichen_alt + 1000000))
                            {
                                zeichen_alt = zeichen;
                                fortschritt = (int)(((zeichen * 10) / fileStr3.Length));

                                if ((fortschritt_merker < fortschritt) && (fortschritt < 101))
                                {
                                    fortschritt_merker = fortschritt;
                                    worker2.ReportProgress((int)fortschritt);
                                }

                            }

                            bildwechsel++;

                            if (bildwechsel > 10000)
                            {
                                if (bildwechsel_merker == 0)
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_aus();
                                    }));
                                    bildwechsel_merker = 1;
                                }
                                else
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_ein();
                                    }));
                                    bildwechsel_merker = 0;
                                }
                                bildwechsel = 0;
                            }

                        }

                        binWriter3.Close();
                        schreib = (byte)(0x00);
                        FileStream fileStr4 = new FileStream(@dateigroesse1, FileMode.Open, FileAccess.Write);
                        BinaryWriter binWriter4 = new BinaryWriter(fileStr4);
                        for (long i2 = 0; i2 < fileStr4.Length; i2++)
                        {
                            //binWriter4.Write((schreib));
                            fileStr4.WriteByte(schreib);
                            zeichen++;
                            if ((zeichen) > (zeichen_alt + 100000))
                            {
                                zeichen_alt = zeichen;
                                fortschritt = (int)(((zeichen * 10) / fileStr4.Length));
                                if ((fortschritt_merker < fortschritt) && (fortschritt < 101))
                                {

                                    fortschritt_merker = fortschritt;
                                    worker2.ReportProgress((int)fortschritt);
                                    // MessageBox.Show("Prozent = " + (warte*2));
                                }



                            }


                            bildwechsel++;

                            if (bildwechsel > 10000)
                            {
                                if (bildwechsel_merker == 0)
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_aus();
                                    }));
                                    bildwechsel_merker = 1;
                                }
                                else
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_ein();
                                    }));
                                    bildwechsel_merker = 0;
                                }
                                bildwechsel = 0;
                            }

                        }
                        binWriter4.Close();
                        schreib = (byte)(0xAA);
                        FileStream fileStr5 = new FileStream(@dateigroesse1, FileMode.Open, FileAccess.Write);
                        BinaryWriter binWriter5 = new BinaryWriter(fileStr5);
                        for (long i3 = 0; i3 < fileStr5.Length; i3++)
                        {
                            //binWriter5.Write((schreib));
                            fileStr5.WriteByte(schreib);

                            zeichen++;
                            if ((zeichen) > (zeichen_alt + 100000))
                            {
                                zeichen_alt = zeichen;
                                fortschritt = (int)(((zeichen * 10) / fileStr5.Length));
                                if ((fortschritt_merker < fortschritt) && (fortschritt < 101))
                                {

                                    fortschritt_merker = fortschritt;
                                    worker2.ReportProgress((int)fortschritt);
                                    // MessageBox.Show("Prozent = " + (warte*2));
                                }


                            }


                            bildwechsel++;

                            if (bildwechsel > 10000)
                            {
                                if (bildwechsel_merker == 0)
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_aus();
                                    }));
                                    bildwechsel_merker = 1;
                                }
                                else
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_ein();
                                    }));
                                    bildwechsel_merker = 0;
                                }
                                bildwechsel = 0;
                            }

                        }

                        binWriter5.Close();

                        schreib = (byte)(0x55);
                        FileStream fileStr6 = new FileStream(@dateigroesse1, FileMode.Open, FileAccess.Write);
                        BinaryWriter binWriter6 = new BinaryWriter(fileStr6);

                        for (long i4 = 0; i4 < fileStr6.Length; i4++)
                        {
                            //binWriter6.Write((schreib));
                            fileStr6.WriteByte(schreib);
                            zeichen++;
                            if ((zeichen) > (zeichen_alt + 100000))
                            {
                                zeichen_alt = zeichen;
                                fortschritt = (int)(((zeichen * 10) / fileStr6.Length));

                                if ((fortschritt_merker < fortschritt) && (fortschritt < 101))
                                {

                                    fortschritt_merker = fortschritt;
                                    worker2.ReportProgress((int)fortschritt);
                                    // MessageBox.Show("Prozent = " + (warte*2));
                                }


                            }


                            bildwechsel++;

                            if (bildwechsel > 10000)
                            {
                                if (bildwechsel_merker == 0)
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_aus();
                                    }));
                                    bildwechsel_merker = 1;
                                }
                                else
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_ein();
                                    }));
                                    bildwechsel_merker = 0;
                                }
                                bildwechsel = 0;
                            }

                        }

                        binWriter6.Close();
                        schreib = (byte)(0x00);
                        FileStream fileStr7 = new FileStream(@dateigroesse1, FileMode.Open, FileAccess.Write);
                        BinaryWriter binWriter7 = new BinaryWriter(fileStr7);


                        for (long i5 = 0; i5 < fileStr7.Length; i5++)
                        {


                            //binWriter7.Write(schreib);
                            fileStr7.WriteByte(schreib);

                            zeichen++;
                            if ((zeichen) > (zeichen_alt + 100000))
                            {
                                zeichen_alt = zeichen;
                                fortschritt = (int)(((zeichen * 10) / fileStr7.Length));

                                if ((fortschritt_merker < fortschritt) && (fortschritt < 101))
                                {

                                    fortschritt_merker = fortschritt;
                                    worker2.ReportProgress((int)fortschritt);
                                    // MessageBox.Show("Prozent = " + (warte*2));
                                }


                            }


                            bildwechsel++;

                            if (bildwechsel > 10000)
                            {
                                if (bildwechsel_merker == 0)
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_aus();
                                    }));
                                    bildwechsel_merker = 1;
                                }
                                else
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_ein();
                                    }));
                                    bildwechsel_merker = 0;
                                }
                                bildwechsel = 0;
                            }

                        }

                        binWriter7.Close();

                    }

                    File.Delete(@dateigroesse1);
                    worker2.ReportProgress((int)100);
                    ShowFg("Secure deletion is completed successfully!");
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                    {
                        flame_aus();
                    }));

                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                    {
                        textBox1.Text = "";
                        textBox2.Text = "";
                    }));
                    worker2.ReportProgress((int)0);
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                    {
                        label5.Foreground = Brushes.Black;
                        SetBurningLogo(false);
                        // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                    }));
                    fortschritt = 0;
                    prozess_laueft = false;

                }
                else
                {
                    prozess_laueft = false;

                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                    {
                        flame_aus();
                    }));
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                    {
                        label5.Foreground = Brushes.Black;
                        SetBurningLogo(false);
                        // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                    }));
                }

            }

            catch
            {
                ShowFg("Deleting not possible! Check file properties and permissions!");
                prozess_laueft = false;
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    flame_aus();
                }));
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    label5.Foreground = Brushes.Black;
                    SetBurningLogo(false);
                    // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                }));
            }



        }




        private void backgroundWorker1_DoWork(object sender,
         DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            // Assign the result of the computation
            // to the Result property of the DoWorkEventArgs
            // object. This is will be available to the 
            // RunWorkerCompleted eventhandler.

            //textBox5.Text = "Background";

            /*_______________TURBINESTART__________________________*/




            long warte = 0;

            byte[] passwort_gen_vector = new byte[100];
            byte[] passwort_gen_vector2 = new byte[100];
            byte[] passwort_gen_vector3 = new byte[100];
            byte[] passwort_gen_vector4 = new byte[100];

            byte[] gear_a = new byte[30];
            byte[] gear_b = new byte[30];
            byte[] gear_c = new byte[30];

            byte[] gear_a2 = new byte[30];
            byte[] gear_b2 = new byte[30];
            byte[] gear_c2 = new byte[30];

            byte[] gear_a3 = new byte[30];
            byte[] gear_b3 = new byte[30];
            byte[] gear_c3 = new byte[30];

            byte[] gear_a4 = new byte[30];
            byte[] gear_b4 = new byte[30];
            byte[] gear_c4 = new byte[30];

            byte gear_ergebnisa1 = 0;
            byte gear_ergebnisa2 = 0;
            byte gear_ergebnisa3 = 0;
            byte gear_ergebnisa4 = 0;
            byte gear_ergebnisa5 = 0;
            byte gear_ergebnisa6 = 0;
            byte gear_ergebnisa7 = 0;
            byte gear_ergebnisa8 = 0;
            byte gear_ergebnisa9 = 0;
            byte gear_ergebnisa10 = 0;

            byte gear_ergebnisa1_2 = 0;
            byte gear_ergebnisa2_2 = 0;
            byte gear_ergebnisa3_2 = 0;
            byte gear_ergebnisa4_2 = 0;
            byte gear_ergebnisa5_2 = 0;
            byte gear_ergebnisa6_2 = 0;
            byte gear_ergebnisa7_2 = 0;
            byte gear_ergebnisa8_2 = 0;
            byte gear_ergebnisa9_2 = 0;
            byte gear_ergebnisa10_2 = 0;


            byte gear_ergebnisa1_3 = 0;
            byte gear_ergebnisa2_3 = 0;
            byte gear_ergebnisa3_3 = 0;
            byte gear_ergebnisa4_3 = 0;
            byte gear_ergebnisa5_3 = 0;
            byte gear_ergebnisa6_3 = 0;
            byte gear_ergebnisa7_3 = 0;
            byte gear_ergebnisa8_3 = 0;
            byte gear_ergebnisa9_3 = 0;
            byte gear_ergebnisa10_3 = 0;

            byte gear_ergebnisa1_4 = 0;
            byte gear_ergebnisa2_4 = 0;
            byte gear_ergebnisa3_4 = 0;
            byte gear_ergebnisa4_4 = 0;
            byte gear_ergebnisa5_4 = 0;
            byte gear_ergebnisa6_4 = 0;
            byte gear_ergebnisa7_4 = 0;
            byte gear_ergebnisa8_4 = 0;
            byte gear_ergebnisa9_4 = 0;
            byte gear_ergebnisa10_4 = 0;









            byte gear_ergebnisb1 = 0;
            byte gear_ergebnisb2 = 0;
            byte gear_ergebnisb3 = 0;
            byte gear_ergebnisb4 = 0;
            byte gear_ergebnisb5 = 0;
            byte gear_ergebnisb6 = 0;
            byte gear_ergebnisb7 = 0;
            byte gear_ergebnisb8 = 0;

            byte gear_ergebnisb1_2 = 0;
            byte gear_ergebnisb2_2 = 0;
            byte gear_ergebnisb3_2 = 0;
            byte gear_ergebnisb4_2 = 0;
            byte gear_ergebnisb5_2 = 0;
            byte gear_ergebnisb6_2 = 0;
            byte gear_ergebnisb7_2 = 0;
            byte gear_ergebnisb8_2 = 0;

            byte gear_ergebnisb1_3 = 0;
            byte gear_ergebnisb2_3 = 0;
            byte gear_ergebnisb3_3 = 0;
            byte gear_ergebnisb4_3 = 0;
            byte gear_ergebnisb5_3 = 0;
            byte gear_ergebnisb6_3 = 0;
            byte gear_ergebnisb7_3 = 0;
            byte gear_ergebnisb8_3 = 0;



            byte gear_ergebnisb1_4 = 0;
            byte gear_ergebnisb2_4 = 0;
            byte gear_ergebnisb3_4 = 0;
            byte gear_ergebnisb4_4 = 0;
            byte gear_ergebnisb5_4 = 0;
            byte gear_ergebnisb6_4 = 0;
            byte gear_ergebnisb7_4 = 0;
            byte gear_ergebnisb8_4 = 0;







            byte gear_ergebnisc1 = 0;
            byte gear_ergebnisc2 = 0;
            byte gear_ergebnisc3 = 0;
            byte gear_ergebnisc4 = 0;

            byte gear_ergebnisc1_2 = 0;
            byte gear_ergebnisc2_2 = 0;
            byte gear_ergebnisc3_2 = 0;
            byte gear_ergebnisc4_2 = 0;

            byte gear_ergebnisc1_3 = 0;
            byte gear_ergebnisc2_3 = 0;
            byte gear_ergebnisc3_3 = 0;
            byte gear_ergebnisc4_3 = 0;

            byte gear_ergebnisc1_4 = 0;
            byte gear_ergebnisc2_4 = 0;
            byte gear_ergebnisc3_4 = 0;
            byte gear_ergebnisc4_4 = 0;



            byte gear_ergebnisd1 = 0;
            byte gear_ergebnisd2 = 0;


            byte gear_ergebnisd1_2 = 0;
            byte gear_ergebnisd2_2 = 0;

            byte gear_ergebnisd1_3 = 0;
            byte gear_ergebnisd2_3 = 0;

            byte gear_ergebnisd1_4 = 0;
            byte gear_ergebnisd2_4 = 0;



            byte gear_ergebnise1 = 0;
            /*byte gear_ergebnise1_alt1 = 0;
            byte gear_ergebnise1_alt2 = 0;
            byte gear_ergebnise1_alt3 = 0;
            byte gear_ergebnise1_alt4 = 0;
            byte gear_ergebnise1_alt5 = 0;

            byte gear_ergebnise1_alt6 = 0;
            byte gear_ergebnise1_alt7 = 0;
            byte gear_ergebnise1_alt8 = 0;
            byte gear_ergebnise1_alt9 = 0;
            byte gear_ergebnise1_alt10 = 0;*/



            byte gear_ergebnise1_2 = 0;

            byte gear_ergebnise1_3 = 0;

            byte gear_ergebnise1_4 = 0;



            byte takt = 0;
            /*byte zeichenmerker = 0;
            byte temp_gear_a = 0;
            byte temp_gear_b = 0;
            byte temp_gear_c = 0;          
            byte temp_gear_b2 = 0;
            byte temp_gear_c2 = 0;
            int temp_u16 = 0;*/

            byte temp_gear_a2 = 0;


            long passwortzaehler = 0;
            long passwortzaehler2 = 0;
            long passwortzaehler3 = 0;

            byte temp_passwort_gen = 0;
            byte temp_passwort_gen2 = 0;
            byte temp_passwort_gen3 = 0;
            byte temp_passwort_gen4 = 0;
            byte temp_passwort_gen5 = 0;

            /*byte passwort = 0;
            byte passwort_byte1 = 0;
            byte passwort_byte2 = 0;
            byte passwort_byte3 = 0;*/

            long passwort_laenge = 0;
            long passwort_ascii_addiert = 0;
            long passwort_ascii_wertig_addiert = 0;
            long passwort_kleinster_wert = 1000;
            long passwort_groesster_wert = 0;
            long laufe = 0;

            byte passwort_info_byte = 0;

            byte passwort_info_byte2 = 0;

            byte passwort_info_byte3 = 0;

            byte passwort_info_byte3_2 = 0;

            byte passwort_info_byte3_3 = 0;

            byte passwort_info_byte3_4 = 0;

            byte passwort_info_byte3_5 = 0;

            byte passwort_info_byte3_6 = 0;



            byte passwort_info_byte4 = 0;

            byte passwort_wippe = 0;

            byte wippe_merker = 0;


            byte passwort_wippe2 = 0;
            byte wippe_merker2 = 0;

            byte bytemerker = 0;

            byte[] zeichenbuffer = new byte[300];
            byte[] gearbuffer = new byte[300];
            byte zeichenanzahl = 0;

            ushort[] bitschieber = new ushort[300];
            ushort[] bitschieber2 = new ushort[300];
            byte[] bitschieber3 = new byte[300];
            byte[] bitschieber4 = new byte[300];
           
       
            byte schiebweite = 0;
            byte schiebmerker = 0;
            byte block_quersumme = 0;
            byte block_quersumme_merker = 0;
            byte block_summe = 0;
            byte block_summe_merker = 0;
            byte erster_durchlauf = 0;
            byte erster_durchlauf_ent = 0;


            long zeichenmenge = 0;

            byte bald_ende = 0;

            byte block_laenge = 8;

            byte block_modulo = 0;

            byte[] zufall = new byte[16];
            // 2026-07-08: Gehaertete IV-Erzeugung (CNG + unabhaengiger Timing-Jitter, SHA-256).
            // Defense-in-Depth gegen einen manipulierten OS-CSPRNG; format-transparent, kein
            // Versions-Byte noetig. Details/Untersuchung: IV_HARDENING.md, Impl.: Window1.IvHardening.cs
            GenerateIV16(zufall);
            // --- vorher (reiner OS-CSPRNG):
            // using (var rng = new RNGCryptoServiceProvider())
            // {
            //     rng.GetBytes(zufall);
            // }

            // ===== KDF V2/V3/V4/V5.2 - Schluessel-Ableitung mit Whitening =====
            // Versions-Byte im BMP-Header (Position 6, normalerweise "reserved"):
            //   0x00 = Legacy V1 (alte Dateien, kein KDF)
            //   0x01 = V2 Passwort + PBKDF2-SHA512 mit IV als Salt
            //   0x02 = V2 Schluesseldatei-Modus (raw bytes, kein Whitening - Legacy)
            //   0x03 = V3 Schluesseldatei-Modus mit SHA-512-Whitening (neuer Default fuer Key-Files)
            //   0x04 = V4 Passwort + PBKDF2 + korrigierte Gear-Masken (LSB-Bias-Fix)
            //   0x05 = V4 Schluesseldatei + SHA-512-Whitening + korrigierte Gear-Masken
            //   0x06 = V5.2 Passwort + verbesserte Passwort-Info-Bytes (Verkettung, kein Shift)
            //   0x07 = V5.2 Schluesseldatei + verbesserte Passwort-Info-Bytes
            byte version_byte_to_write = 0x00;
            byte version_byte_of_file = 0x00;

            if (richtung_info == 0)
            {
                // Verschluesselung: V5.2 mit verbesserten Passwort-Info-Bytes (0x06 Passwort, 0x07 Key-File)
                version_byte_to_write = (schluesseldatei_geladen == 1) ? (byte)0x07 : (byte)0x06;
            }
            else
            {
                // Entschluesselung: Versions-Byte und IV vorab aus Datei lesen
                try
                {
                    using (var pre_fs = new FileStream(dateigroesse1, FileMode.Open, FileAccess.Read))
                    {
                        if (pre_fs.Length >= 70)
                        {
                            pre_fs.Seek(6, SeekOrigin.Begin);
                            version_byte_of_file = (byte)pre_fs.ReadByte();
                            if (version_byte_of_file == 0x01 || version_byte_of_file == 0x02 || version_byte_of_file == 0x03
                                || version_byte_of_file == 0x04 || version_byte_of_file == 0x05
                                || version_byte_of_file == 0x06 || version_byte_of_file == 0x07)
                            {
                                pre_fs.Seek(54, SeekOrigin.Begin);
                                pre_fs.Read(zufall, 0, 16);
                            }
                        }
                    }
                }
                catch
                {
                    // bei Lese-Fehler: Legacy-Modus annehmen
                    version_byte_of_file = 0x00;
                }
            }

            // ----- V2/V4/V5.2: PBKDF2 fuer Passwort-Modus (Versions-Byte 0x01, 0x04 oder 0x06) -----
            bool use_pbkdf2 = false;
            if (richtung_info == 0 && schluesseldatei_geladen == 0)
            {
                use_pbkdf2 = true;
            }
            if (richtung_info == 1 && (version_byte_of_file == 0x01 || version_byte_of_file == 0x04 || version_byte_of_file == 0x06))
            {
                use_pbkdf2 = true;
            }

            if (use_pbkdf2)
            {
                // ReportProgress damit UI sieht dass etwas passiert (KDF dauert ~5-10 Sek)
                if (backgroundWorker1 != null && backgroundWorker1.WorkerReportsProgress)
                {
                    backgroundWorker1.ReportProgress(1);
                }

                // Schritt 1: Master-Key-Extraktion mit PBKDF2-SHA512 (LANGSAM, einmalig)
                // PBKDF2 in separatem Task ausfuehren, damit wir parallel Fortschritt melden koennen.
                // Sonst waere der Balken fuer ~5 Sek bei 1% eingefroren.
                byte[] master_key = null;
                var kdf_task = System.Threading.Tasks.Task.Run(() =>
                {
                    using (var pbkdf2 = new Rfc2898DeriveBytes(passwort1, zufall, 1200000, HashAlgorithmName.SHA512))
                    {
                        master_key = pbkdf2.GetBytes(64);
                    }
                });

                // Parallel zur PBKDF2-Berechnung: alle 500ms eine Progress-Erhoehung
                // bis maximal 8% (die eigentliche Verschluesselung laeuft danach von 8% bis 100%)
                int kdf_progress = 1;
                while (!kdf_task.IsCompleted)
                {
                    System.Threading.Thread.Sleep(500);
                    if (kdf_progress < 8)
                    {
                        kdf_progress++;
                        if (backgroundWorker1 != null && backgroundWorker1.WorkerReportsProgress)
                        {
                            backgroundWorker1.ReportProgress(kdf_progress);
                        }
                    }
                }
                kdf_task.Wait(); // Sicherstellen dass Task wirklich beendet und Exceptions sichtbar

                // Schritt 2: Expansion auf 1024 Byte mit Counter-Mode SHA-512 (SCHNELL)
                byte[] expanded = new byte[1024];
                for (int i = 0; i < 16; i++)
                {
                    using (var sha = SHA512.Create())
                    {
                        byte[] input = new byte[master_key.Length + 1];
                        Array.Copy(master_key, input, master_key.Length);
                        input[master_key.Length] = (byte)i;
                        byte[] block = sha.ComputeHash(input);
                        Array.Copy(block, 0, expanded, i * 64, 64);
                    }
                }
                name_der_datei6 = expanded;
                passwortgroesse = 1024;
                gen_passwort = 1024;
            }

            // ----- V3: SHA-512-Whitening fuer Key-File-Modus (Versions-Byte 0x03) -----
            // Kein PBKDF2 noetig (1024-Byte-Key-File hat schon ~8000 bit Entropie),
            // aber Whitening absorbiert strukturelle Patterns aus JPG/ZIP/etc.
            // Damit verschwinden Approximate-Entropy-FAILs aus V2 Key-File-Modus (0x02).
            bool use_keyfile_whitening = false;
            if (richtung_info == 0 && schluesseldatei_geladen == 1)
            {
                use_keyfile_whitening = true;
            }
            if (richtung_info == 1 && (version_byte_of_file == 0x03 || version_byte_of_file == 0x05 || version_byte_of_file == 0x07))
            {
                use_keyfile_whitening = true;
            }

            if (use_keyfile_whitening)
            {
                // Schritt 1: SHA-512 ueber Key-File-Bytes (1023) + IV (16) = 1039 Bytes Input
                // Der IV macht den Master-Key per-Datei einzigartig, auch wenn das
                // gleiche Key-File mehrfach verwendet wird.
                byte[] kf_input = new byte[1023 + 16];
                Array.Copy(name_der_datei6X, 0, kf_input, 0, 1023);
                Array.Copy(zufall, 0, kf_input, 1023, 16);

                byte[] kf_master_key;
                using (var sha = SHA512.Create())
                {
                    kf_master_key = sha.ComputeHash(kf_input);
                }

                // Schritt 2: Counter-Mode-Expansion auf 1024 Byte (selbes Verfahren wie V2)
                byte[] kf_expanded = new byte[1024];
                for (int i = 0; i < 16; i++)
                {
                    using (var sha = SHA512.Create())
                    {
                        byte[] input = new byte[kf_master_key.Length + 1];
                        Array.Copy(kf_master_key, input, kf_master_key.Length);
                        input[kf_master_key.Length] = (byte)i;
                        byte[] block = sha.ComputeHash(input);
                        Array.Copy(block, 0, kf_expanded, i * 64, 64);
                    }
                }
                name_der_datei6 = kf_expanded;
                passwortgroesse = 1024;
                gen_passwort = 1024;
            }

            // Fortschritts-Balken-Sync: Wenn KDF lief (Progress 1-8%), muss der
            // Verschluesselungs-Loop ab 8% weitermachen (statt bei 0% neu zu starten).
            // Sonst springt der Balken sichtbar zurueck.
            if (use_pbkdf2 || use_keyfile_whitening)
            {
                fortschritt_merker = 8;
            }
            // ===== Ende KDF V2/V3/V4 =====

            // ===== V4: Korrigierte Gear-Masken + Gear-Feedback (Bias-Fix) =====
            // Fix 1 (Masken): In V1-V3 verwenden zwei Stellen 0x55/0xAA statt 0xFF.
            //   Da 0xAA Bit 0 nicht flippt, stagniert das LSB → Monobit/CumSum FAIL.
            // Fix 2 (Feedback): In V1-V3 verwenden gear_a[17], gear_a3[17], gear_b2[0],
            //   gear_c[7], gear_c3[7] Addition (+) statt XOR (^) im Feedback-Loop.
            //   Addition erzeugt Carry-Propagation → Chi²-Byte-Verteilungs-Bias.
            // HINWEIS: gear_ergebnisa5 (gear_b[4]+gear_ergebnisd1) bleibt Addition —
            //   diese Nichtlinearitaet im Keystream verbessert die Mischung.
            // Ab V4 (0x04/0x05) werden Masken auf 0xFF und Additionen auf XOR korrigiert.
            bool use_fixed_masks = false;
            if (richtung_info == 0)
            {
                // Verschluesselung: immer die korrigierten Werte verwenden
                use_fixed_masks = true;
            }
            else
            {
                // Entschluesselung: nur bei V4/V5.2-Dateien
                use_fixed_masks = (version_byte_of_file == 0x04 || version_byte_of_file == 0x05
                                || version_byte_of_file == 0x06 || version_byte_of_file == 0x07);
            }
            byte mask_a3_16 = use_fixed_masks ? (byte)0xFF : (byte)0x55;
            byte mask_a4_16 = use_fixed_masks ? (byte)0xFF : (byte)0xAA;
            byte mask_b4_1  = use_fixed_masks ? (byte)0xFF : (byte)0x55;
            byte mask_b3_1  = use_fixed_masks ? (byte)0xFF : (byte)0xAA;
            // ===== Ende V4 =====

            // ===== V5.2: Verbesserte Passwort-Info-Bytes =====
            // Fix: Rechts-Shifts (>> 3..7) erzeugten bei kurzen Passwoertern zu viele Nullen.
            // Neu: Verkettung (jedes Byte haengt vom vorherigen ab) + XOR 0xFF Inversionen.
            bool use_improved_pw_info = false;
            if (richtung_info == 0)
            {
                use_improved_pw_info = true;
            }
            else
            {
                use_improved_pw_info = (version_byte_of_file == 0x06 || version_byte_of_file == 0x07);
            }
            // ===== Ende V5.2 =====


            try
            {


                if (algo)
                {
                    /*verschlüssele mit Turbine*/
                    passwort_laenge = passwortgroesse;

                    for (laufe = 0; laufe < passwort_laenge; laufe++)
                    {
                        passwort_ascii_addiert = passwort_ascii_addiert + name_der_datei6[laufe];
                    }

                    for (laufe = 0; laufe < passwort_laenge; laufe++)
                    {
                        passwort_ascii_wertig_addiert = passwort_ascii_wertig_addiert + (name_der_datei6[laufe] * (laufe + 1));
                    }

                    for (laufe = 0; laufe < passwort_laenge; laufe++)
                    {
                        if (name_der_datei6[laufe] > passwort_groesster_wert)
                        {
                            passwort_groesster_wert = name_der_datei6[laufe];
                        }
                    }
                    for (laufe = 0; laufe < passwort_laenge; laufe++)
                    {
                        if (name_der_datei6[laufe] < passwort_kleinster_wert)
                        {
                            passwort_kleinster_wert = name_der_datei6[laufe];
                        }
                    }

                    if (use_improved_pw_info)
                    {
                        // V5.2: Verbesserte Passwort-Info-Bytes — keine Rechts-Shifts,
                        // Verkettung fuer Lawineneffekt, XOR 0xFF gegen Null-Haeufung.
                        passwort_info_byte = (byte)(passwort_laenge ^ passwort_ascii_addiert ^ passwort_ascii_wertig_addiert + passwort_kleinster_wert + passwort_groesster_wert);
                        passwort_info_byte2 = (byte)((passwort_laenge + passwort_ascii_addiert + passwort_ascii_wertig_addiert ^ passwort_kleinster_wert ^ passwort_groesster_wert));
                        passwort_info_byte3 = (byte)(passwort_info_byte2 ^ passwort_info_byte);
                        passwort_info_byte3_2 = (byte)((passwort_info_byte3 ^ passwort_ascii_addiert + passwort_ascii_wertig_addiert ^ passwort_kleinster_wert ^ passwort_groesster_wert));
                        passwort_info_byte3_3 = (byte)((passwort_info_byte3_2 ^ passwort_ascii_addiert ^ passwort_ascii_wertig_addiert + passwort_kleinster_wert + passwort_groesster_wert) ^ 0xFF);
                        passwort_info_byte3_4 = (byte)((passwort_info_byte3_3 ^ passwort_ascii_addiert ^ passwort_ascii_wertig_addiert ^ passwort_kleinster_wert + passwort_groesster_wert));
                        passwort_info_byte3_5 = (byte)((passwort_info_byte3_4 ^ passwort_ascii_addiert + passwort_ascii_wertig_addiert + passwort_kleinster_wert ^ passwort_groesster_wert) ^ 0xFF);
                        passwort_info_byte3_6 = (byte)((passwort_info_byte3_5 ^ passwort_ascii_addiert ^ passwort_ascii_wertig_addiert ^ passwort_kleinster_wert ^ passwort_groesster_wert));
                    }
                    else
                    {
                        // Legacy V1-V5.1: Original-Berechnung mit Rechts-Shifts
                        passwort_info_byte = (byte)(passwort_laenge + passwort_ascii_addiert + passwort_ascii_wertig_addiert + passwort_kleinster_wert + passwort_groesster_wert);
                        passwort_info_byte2 = (byte)((passwort_laenge + passwort_ascii_addiert + passwort_ascii_wertig_addiert + passwort_kleinster_wert + passwort_groesster_wert) >> 8);
                        passwort_info_byte3 = (byte)(passwort_info_byte2 ^ passwort_info_byte);
                        passwort_info_byte3_2 = (byte)((passwort_laenge ^ passwort_ascii_addiert + passwort_ascii_wertig_addiert ^ passwort_kleinster_wert ^ passwort_groesster_wert) >> 3);
                        passwort_info_byte3_3 = (byte)((passwort_laenge ^ passwort_ascii_addiert ^ passwort_ascii_wertig_addiert + passwort_kleinster_wert + passwort_groesster_wert) >> 4);
                        passwort_info_byte3_4 = (byte)((passwort_laenge + passwort_ascii_addiert ^ passwort_ascii_wertig_addiert ^ passwort_kleinster_wert + passwort_groesster_wert) >> 5);
                        passwort_info_byte3_5 = (byte)((passwort_laenge ^ passwort_ascii_addiert + passwort_ascii_wertig_addiert + passwort_kleinster_wert ^ passwort_groesster_wert) >> 7);
                        passwort_info_byte3_6 = (byte)((passwort_laenge ^ passwort_ascii_addiert ^ passwort_ascii_wertig_addiert ^ passwort_kleinster_wert ^ passwort_groesster_wert) >> 2);
                    }
                    /*
                    block_laenge = (byte)((passwort_info_byte) % 16);

                    if (block_laenge < 4)
                    {
                        block_laenge = 4;
                    }*/

                    /*
                    MessageBox.Show(" Länge "+passwort_laenge);
               
                    MessageBox.Show(" Addiert ="+passwort_ascii_addiert);
                    MessageBox.Show("  Addiert wertig="+passwort_ascii_wertig_addiert );
                
                    MessageBox.Show("   Minimum="+passwort_kleinster_wert);

                    MessageBox.Show("   Maximum="+passwort_groesster_wert );

                    MessageBox.Show("   Ergebnis="+passwort_info_byte );
                    MessageBox.Show("   Ergebnis=" + passwort_info_byte2);
                    MessageBox.Show("   Ergebnis=" + passwort_info_byte3);
                    MessageBox.Show("   Ergebnis=" + passwort_info_byte3_2);
                    MessageBox.Show("   Ergebnis=" + passwort_info_byte3_3);
                    MessageBox.Show("   Ergebnis=" + passwort_info_byte3_4);
                    MessageBox.Show("   Ergebnis=" + passwort_info_byte3_5);
                    MessageBox.Show("   Ergebnis=" + passwort_info_byte3_6);
                    MessageBox.Show("   richtung_info=" + richtung_info);   
                    */
                    //progressBar1.Value = 0;

                    if (((passwort1.Equals(passwort2) || (radioButton1_global)) && (passwortgroesse <= 1024)) && (passwortgroesse > 5) && (dateil1 > 0) && (dateil2 > 0) && (!(dateigroesse1.Equals(dateigroesse2))))
                    {
                        /*Erstellung der Turbinen-Initialisierung*/

                        /*-----------------------------1.Zahlenr„dergruppe----------------------------------------*/
                        /*~T*/


                        /*printf("\nPasswortlaenge: %d\n",gen_passwort);*/
                        passwortende = 1500 + (name_der_datei6[0] * 3);

                        /*~T*/

                        /*~L:5*/
                        for (passwortzaehler = 0; passwortzaehler < gen_passwort; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[passwortzaehler] * 2);
                            /*printf("\npasswortende: %d\n",passwortende);*/

                            /*~-1*/
                        }
                        /*~E:L5*/
                        /*~I:6*/
                        if (gen_passwort > 2)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[1] + name_der_datei6[2]);
                            /*~-1*/
                        }
                        /*~E:I6*/
                        /*~T*/

                        /*~I:7*/
                        if (gen_passwort > 3)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende - (name_der_datei6[3] * 2);
                            /*~-1*/
                        }
                        /*~E:I7*/
                        /*~I:8*/
                        if (gen_passwort > 4)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[4] * 2);
                            /*~-1*/
                        }
                        /*~E:I8*/
                        /*~I:9*/
                        if (gen_passwort > 5)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[5] ^ name_der_datei6[2]) * 2);
                            /*~-1*/
                        }
                        /*~E:I9*/
                        /*~I:10*/
                        if (gen_passwort > 6)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[6] ^ name_der_datei6[1]) * 2);
                            /*~-1*/
                        }
                        /*~E:I10*/
                        /*~T*/



                        /*~L:11*/
                        for (passwortzaehler = 0; passwortzaehler < passwortende; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~I:12*/
                            if (passwortzaehler2 == 40)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler2 = 0;
                                /*~-1*/
                            }
                            /*~E:I12*/
                            /*~I:13*/
                            if (passwortzaehler3 == gen_passwort)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler3 = 0;
                                /*~-1*/
                            }
                            /*~E:I13*/
                            /*~T*/

                            // V5.2: Schieberegister mit Passwort-Info-Bytes vorinitialisieren (Vektor 1)
                            if (use_improved_pw_info && passwortzaehler2 == 0)
                            {
                                temp_passwort_gen5 = passwort_info_byte;
                                temp_passwort_gen4 = passwort_info_byte2;
                                temp_passwort_gen3 = passwort_info_byte3;
                                temp_passwort_gen2 = passwort_info_byte3_2;
                                temp_passwort_gen  = passwort_info_byte3_3;
                                passwort_gen_vector[passwortzaehler2] = passwort_info_byte3_4;
                            }

                            passwort_gen_vector[passwortzaehler2] = (byte)(((byte)passwort_gen_vector[passwortzaehler2] ^ (byte)name_der_datei6[passwortzaehler3]) ^ (((byte)temp_passwort_gen ^ (byte)temp_passwort_gen2) + ((byte)temp_passwort_gen3 ^ (byte)temp_passwort_gen4 ^ (byte)temp_passwort_gen5)));

                            passwortzaehler2++;
                            passwortzaehler3++;
                            temp_passwort_gen5 = temp_passwort_gen4;
                            temp_passwort_gen4 = temp_passwort_gen3;
                            temp_passwort_gen3 = (byte)(temp_passwort_gen2 + 37);
                            //temp_passwort_gen2=temp_passwort_gen;


                            /*~I:14*/
                            if ((temp_passwort_gen & 0x1) == 0x1)
                            /*~-1*/
                            {
                                /*~T*/
                                temp_passwort_gen2 = temp_passwort_gen;
                                /*~-1*/
                            }
                            /*~O:I14*/
                            /*~-2*/
                            else
                            {
                                /*~T*/
                                temp_passwort_gen2 = (byte)(temp_passwort_gen ^ ((0xFF)));
                                /*~-1*/
                            }
                            /*~E:I14*/
                            /*~T*/


                            temp_passwort_gen = passwort_gen_vector[passwortzaehler2];



                            /*~-1*/
                        }
                        /*~E:L11*/
                        /*~T*/
                        /*printf("\nDurchl„ufe: %d\n",passwortzaehler);*/

                        /*~L:15*/
                        for (passwortzaehler = 0; passwortzaehler < 40; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/


                            /*~I:16*/
                            if (passwortzaehler < 18)
                            /*~-1*/
                            {
                                /*~T*/
                                gear_a[(byte)passwortzaehler] = passwort_gen_vector[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,passwort_gen_vector[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I16*/
                            /*~I:17*/
                            if ((passwortzaehler >= 18) && (passwortzaehler < 32))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_b[((byte)passwortzaehler) - 18] = passwort_gen_vector[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,passwort_gen_vector[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I17*/
                            /*~I:18*/
                            if ((passwortzaehler >= 32) && (passwortzaehler < 40))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_c[((byte)passwortzaehler) - 32] = passwort_gen_vector[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,passwort_gen_vector[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I18*/
                            /*~T*/



                            /*~-1*/
                        }
                        /*~E:L15*/
                        /*~E:A4*/
                        /*~A:19*/



                        /*~+:2.Zahlenr„dergruppe*/
                        /*~T*/
                        /*-----------------------------2.Zahlenr„dergruppe----------------------------------------*/
                        /*~T*/
                        passwortzaehler2 = 0;
                        passwortzaehler3 = 0;
                        passwortende = 1200 + (name_der_datei6[0] * 2);

                        /*~T*/

                        /*~L:20*/
                        for (passwortzaehler = 0; passwortzaehler < gen_passwort; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[passwortzaehler] * 3);
                            /*printf("\npasswortende: %d\n",passwortende);*/
                            /*~-1*/
                        }
                        /*~E:L20*/
                        /*~I:21*/
                        if (gen_passwort > 2)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[1] + name_der_datei6[2]);
                            /*~-1*/
                        }
                        /*~E:I21*/
                        /*~T*/

                        /*~I:22*/
                        if (gen_passwort > 3)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende - (name_der_datei6[3] * 3);
                            /*~-1*/
                        }
                        /*~E:I22*/
                        /*~I:23*/
                        if (gen_passwort > 4)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[4] * 3);
                            /*~-1*/
                        }
                        /*~E:I23*/
                        /*~I:24*/
                        if (gen_passwort > 5)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[5] ^ name_der_datei6[3]) * 2);
                            /*~-1*/
                        }
                        /*~E:I24*/
                        /*~I:25*/
                        if (gen_passwort > 6)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[6] ^ name_der_datei6[2]) * 2);
                            /*~-1*/
                        }
                        /*~E:I25*/
                        /*~T*/



                        /*~L:26*/
                        for (passwortzaehler = 0; passwortzaehler < passwortende; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~I:27*/
                            if (passwortzaehler2 == 40)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler2 = 0;
                                /*~-1*/
                            }
                            /*~E:I27*/
                            /*~I:28*/
                            if (passwortzaehler3 == gen_passwort)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler3 = 0;
                                /*~-1*/
                            }
                            /*~E:I28*/
                            /*~T*/

                            // V5.2: Schieberegister mit Passwort-Info-Bytes vorinitialisieren (Vektor 2)
                            if (use_improved_pw_info && passwortzaehler2 == 0)
                            {
                                temp_passwort_gen5 = passwort_info_byte3_2;
                                temp_passwort_gen4 = passwort_info_byte3_3;
                                temp_passwort_gen3 = passwort_info_byte3_4;
                                temp_passwort_gen2 = passwort_info_byte3_5;
                                temp_passwort_gen  = passwort_info_byte3_6;
                                passwort_gen_vector2[passwortzaehler2] = passwort_info_byte;
                            }

                            passwort_gen_vector2[passwortzaehler2] = (byte)(((byte)(passwort_gen_vector2[passwortzaehler2] ^ name_der_datei6[passwortzaehler3])) ^ (((byte)(temp_passwort_gen ^ temp_passwort_gen2)) + ((byte)(temp_passwort_gen3 ^ temp_passwort_gen4 ^ temp_passwort_gen5))));

                            passwortzaehler2++;
                            passwortzaehler3++;


                            temp_passwort_gen5 = temp_passwort_gen4;
                            if (use_improved_pw_info)
                                temp_passwort_gen4 = (byte)((temp_passwort_gen3 << 3) | (temp_passwort_gen3 >> 5));
                            else
                                temp_passwort_gen4 = (byte)(temp_passwort_gen3 << 3);
                            temp_passwort_gen3 = (byte)(temp_passwort_gen2 + 23);
                            temp_passwort_gen2 = temp_passwort_gen;
                            temp_passwort_gen = passwort_gen_vector2[passwortzaehler2];



                            /*~-1*/
                        }
                        /*~E:L26*/
                        /*~T*/

                        /*~T*/
                        /*printf("\nDurchl„ufe: %d\n",passwortzaehler);*/
                        /*~L:29*/
                        for (passwortzaehler = 0; passwortzaehler < 40; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/


                            /*~I:30*/
                            if (passwortzaehler < 18)
                            /*~-1*/
                            {
                                /*~T*/
                                gear_a2[(byte)passwortzaehler] = passwort_gen_vector2[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,passwort_gen_vector2[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I30*/
                            /*~I:31*/
                            if ((passwortzaehler >= 18) && (passwortzaehler < 32))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_b2[((byte)passwortzaehler) - 18] = passwort_gen_vector2[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,passwort_gen_vector2[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I31*/
                            /*~I:32*/
                            if ((passwortzaehler >= 32) && (passwortzaehler < 40))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_c2[((byte)passwortzaehler) - 32] = passwort_gen_vector2[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,passwort_gen_vector2[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I32*/
                            /*~T*/



                            /*~-1*/
                        }
                        /*~E:L29*/
                        /*~T*/
                        /*-----------------------------2.Zahlenr„dergruppe----------------------------------------*/
                        /*~E:A19*/
                        /*~T*/

                        /*~T*/
                        /*-----------------------------3.Zahlenr„dergruppe----------------------------------------*/
                        /*~A:33*/
                        /*~+:3.Zahlenr„dergruppe*/
                        /*~T*/
                        /*-----------------------------3.Zahlenr„dergruppe----------------------------------------*/
                        /*~T*/
                        passwortzaehler2 = 0;
                        passwortzaehler3 = 0;
                        passwortende = 1750 + (name_der_datei6[0] * 4);

                        /*~T*/

                        /*~L:34*/
                        for (passwortzaehler = 0; passwortzaehler < gen_passwort; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[passwortzaehler] * 2);
                            /*printf("\npasswortende: %d\n",passwortende);*/
                            /*~-1*/
                        }
                        /*~E:L34*/
                        /*~I:35*/
                        if (gen_passwort > 2)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[1] + name_der_datei6[2]);
                            /*~-1*/
                        }
                        /*~E:I35*/
                        /*~T*/

                        /*~I:36*/
                        if (gen_passwort > 3)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende - ((name_der_datei6[3] ^ name_der_datei6[2]) * 2);
                            /*~-1*/
                        }
                        /*~E:I36*/
                        /*~I:37*/
                        if (gen_passwort > 4)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[4] ^ name_der_datei6[3]) * 2);
                            /*~-1*/
                        }
                        /*~E:I37*/
                        /*~I:38*/
                        if (gen_passwort > 5)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[5] ^ name_der_datei6[0]) * 2);
                            /*~-1*/
                        }
                        /*~E:I38*/
                        /*~I:39*/
                        if (gen_passwort > 6)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[6] & name_der_datei6[2]) * 3);
                            /*~-1*/
                        }
                        /*~E:I39*/
                        /*~T*/



                        /*~L:40*/
                        for (passwortzaehler = 0; passwortzaehler < passwortende; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~I:41*/
                            if (passwortzaehler2 == 40)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler2 = 0;
                                /*~-1*/
                            }
                            /*~E:I41*/
                            /*~I:42*/
                            if (passwortzaehler3 == gen_passwort)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler3 = 0;
                                /*~-1*/
                            }
                            /*~E:I42*/
                            /*~T*/

                            // V5.2: Schieberegister mit Passwort-Info-Bytes vorinitialisieren (Vektor 3)
                            if (use_improved_pw_info && passwortzaehler2 == 0)
                            {
                                temp_passwort_gen5 = passwort_info_byte3_4;
                                temp_passwort_gen4 = passwort_info_byte3_5;
                                temp_passwort_gen3 = passwort_info_byte3_6;
                                temp_passwort_gen2 = passwort_info_byte;
                                temp_passwort_gen  = passwort_info_byte2;
                                passwort_gen_vector3[passwortzaehler2] = passwort_info_byte3;
                            }

                            passwort_gen_vector3[passwortzaehler2] = (byte)(((byte)(passwort_gen_vector3[passwortzaehler2] ^ name_der_datei6[passwortzaehler3])) ^ (((byte)(temp_passwort_gen ^ temp_passwort_gen2)) ^ ((byte)(temp_passwort_gen3 ^ temp_passwort_gen4 ^ temp_passwort_gen5))));

                            passwortzaehler2++;
                            passwortzaehler3++;





                            /*~I:43*/
                            if ((temp_passwort_gen4 & 0x1) == 0x1)
                            /*~-1*/
                            {
                                /*~T*/
                                temp_passwort_gen5 = (byte)(temp_passwort_gen4 ^ temp_passwort_gen3);
                                /*~-1*/
                            }
                            /*~O:I43*/
                            /*~-2*/
                            else
                            {
                                /*~T*/
                                temp_passwort_gen5 = temp_passwort_gen4;
                                /*~-1*/
                            }
                            /*~E:I43*/
                            /*~T*/


                            /*~I:44*/
                            if ((temp_passwort_gen3 & 0x20) == 0x20)
                            /*~-1*/
                            {
                                /*~T*/
                                temp_passwort_gen4 = temp_passwort_gen3;
                                /*~-1*/
                            }
                            /*~O:I44*/
                            /*~-2*/
                            else
                            {
                                /*~T*/
                                temp_passwort_gen4 = (byte)(temp_passwort_gen3 ^ 0xFF);
                                /*~-1*/
                            }
                            /*~E:I44*/
                            /*~T*/


                            /*temp_passwort_gen4=temp_passwort_gen3;*/
                            temp_passwort_gen3 = (byte)(temp_passwort_gen2 + 27);
                            temp_passwort_gen2 = temp_passwort_gen;
                            temp_passwort_gen = passwort_gen_vector3[passwortzaehler2];



                            /*~-1*/
                        }
                        /*~E:L40*/
                        /*~T*/

                        /*~T*/
                        /*printf("\nDurchl„ufe: %d\n",passwortzaehler);*/
                        /*~L:45*/
                        for (passwortzaehler = 0; passwortzaehler < 40; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/


                            /*~I:46*/
                            if (passwortzaehler < 18)
                            /*~-1*/
                            {
                                /*~T*/
                                gear_a3[(byte)passwortzaehler] = passwort_gen_vector3[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,gear_a3[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I46*/
                            /*~I:47*/
                            if ((passwortzaehler >= 18) && (passwortzaehler < 32))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_b3[((byte)passwortzaehler) - 18] = passwort_gen_vector3[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,gear_b3[(passwortzaehler)-18]);


                                /*~-1*/
                            }
                            /*~E:I47*/
                            /*~I:48*/
                            if ((passwortzaehler >= 32) && (passwortzaehler < 40))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_c3[((byte)passwortzaehler) - 32] = passwort_gen_vector3[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,gear_c3[(passwortzaehler)-32]);


                                /*~-1*/
                            }
                            /*~E:I48*/
                            /*~T*/



                            /*~-1*/
                        }
                        /*~E:L45*/
                        /*~T*/
                        /*-----------------------------3.Zahlenr„dergruppe----------------------------------------*/
                        /*~E:A33*/
                        /*~T*/
                        /*-----------------------------3.Zahlenr„dergruppe----------------------------------------*/
                        /*~T*/
                        /*~T*/
                        /*-----------------------------4.Zahlenr„dergruppe----------------------------------------*/
                        /*~A:49*/
                        /*~+:4.Zahlenr„dergruppe*/
                        /*~T*/
                        /*-----------------------------4.Zahlenr„dergruppe----------------------------------------*/
                        /*~T*/
                        passwortzaehler2 = 0;
                        passwortzaehler3 = 0;
                        passwortende = 2700 + (name_der_datei6[0] * 5);

                        /*~T*/

                        /*~L:50*/
                        for (passwortzaehler = 0; passwortzaehler < gen_passwort; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[passwortzaehler] * 1);
                            /*printf("\npasswortende: %d\n",passwortende);*/
                            /*~-1*/
                        }
                        /*~E:L50*/
                        /*~I:51*/
                        if (gen_passwort > 2)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[1] + name_der_datei6[2]);
                            /*~-1*/
                        }
                        /*~E:I51*/
                        /*~T*/

                        /*~I:52*/
                        if (gen_passwort > 3)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende - (name_der_datei6[3] * 4);
                            /*~-1*/
                        }
                        /*~E:I52*/
                        /*~I:53*/
                        if (gen_passwort > 4)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + (name_der_datei6[4] * 2);
                            /*~-1*/
                        }
                        /*~E:I53*/
                        /*~I:54*/
                        if (gen_passwort > 5)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[5] | name_der_datei6[2]) * 2);
                            /*~-1*/
                        }
                        /*~E:I54*/
                        /*~I:55*/
                        if (gen_passwort > 6)
                        /*~-1*/
                        {
                            /*~T*/
                            passwortende = passwortende + ((name_der_datei6[6] | name_der_datei6[1]) * 2);
                            /*~-1*/
                        }
                        /*~E:I55*/
                        /*~T*/



                        /*~L:56*/
                        for (passwortzaehler = 0; passwortzaehler < passwortende; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~I:57*/
                            if (passwortzaehler2 == 40)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler2 = 0;
                                /*~-1*/
                            }
                            /*~E:I57*/
                            /*~I:58*/
                            if (passwortzaehler3 == gen_passwort)
                            /*~-1*/
                            {
                                /*~T*/
                                passwortzaehler3 = 0;
                                /*~-1*/
                            }
                            /*~E:I58*/
                            /*~T*/

                            // V5.2: Schieberegister mit Passwort-Info-Bytes vorinitialisieren (Vektor 4)
                            if (use_improved_pw_info && passwortzaehler2 == 0)
                            {
                                temp_passwort_gen5 = passwort_info_byte3_6;
                                temp_passwort_gen4 = passwort_info_byte3;
                                temp_passwort_gen3 = passwort_info_byte3_2;
                                temp_passwort_gen2 = passwort_info_byte3_3;
                                temp_passwort_gen  = passwort_info_byte3_4;
                                passwort_gen_vector4[passwortzaehler2] = passwort_info_byte3_5;
                            }

                            passwort_gen_vector4[passwortzaehler2] = (byte)((passwort_gen_vector4[passwortzaehler2] ^ name_der_datei6[passwortzaehler3]) ^ ((temp_passwort_gen ^ temp_passwort_gen2) ^ (temp_passwort_gen3 ^ temp_passwort_gen4 ^ temp_passwort_gen5)));

                            passwortzaehler2++;
                            passwortzaehler3++;


                            temp_passwort_gen5 = (byte)(temp_passwort_gen4 + passwortzaehler);
                            if (use_improved_pw_info)
                                temp_passwort_gen4 = (byte)((temp_passwort_gen3 << 2) | (temp_passwort_gen3 >> 6));
                            else
                                temp_passwort_gen4 = (byte)(temp_passwort_gen3 << 2);
                            temp_passwort_gen3 = (byte)(temp_passwort_gen2 + 31);
                            temp_passwort_gen2 = temp_passwort_gen;
                            temp_passwort_gen = passwort_gen_vector4[passwortzaehler2];



                            /*~-1*/
                        }
                        /*~E:L56*/
                        /*~T*/

                        /*~T*/
                        /*printf("\nDurchl„ufe: %d\n",passwortzaehler);*/
                        /*~L:59*/
                        for (passwortzaehler = 0; passwortzaehler < 40; passwortzaehler++)
                        /*~-1*/
                        {
                            /*~T*/


                            /*~I:60*/
                            if (passwortzaehler < 18)
                            /*~-1*/
                            {
                                /*~T*/
                                gear_a4[(byte)passwortzaehler] = passwort_gen_vector4[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler,gear_a4[passwortzaehler]);


                                /*~-1*/
                            }
                            /*~E:I60*/
                            /*~I:61*/
                            if ((passwortzaehler >= 18) && (passwortzaehler < 32))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_b4[((byte)passwortzaehler) - 18] = passwort_gen_vector4[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler-18,gear_b4[passwortzaehler-18]);


                                /*~-1*/
                            }
                            /*~E:I61*/
                            /*~I:62*/
                            if ((passwortzaehler >= 32) && (passwortzaehler < 40))
                            /*~-1*/
                            {
                                /*~T*/
                                gear_c4[((byte)passwortzaehler) - 32] = passwort_gen_vector4[passwortzaehler];
                                //printf("\nByte %d = %x\n",passwortzaehler-32,gear_c4[passwortzaehler-32]);

                                /*~-1*/
                            }
                            /*~E:I62*/
                            /*~T*/



                            /*~-1*/
                        }
                        /*~E:L59*/
                        /*~T*/
                        /*-----------------------------4.Zahlenr„dergruppe----------------------------------------*/
                        /*~E:A49*/
                        /*~T*/
                        /*-----------------------------4.Zahlenr„dergruppe----------------------------------------*/
                        /*~T*/

                        /*Ende Erstellung der Turbinen-Initialisierung*/




                        /*___________________________________________________________________________*/


                        // eine Datei erzeugen und einen Integer-Wert in 
                        // die Datei schreiben 
                        FileStream fileStr2 = new FileStream(@dateigroesse2,
                        FileMode.Create);
                        BinaryWriter binWriter2 = new BinaryWriter(fileStr2);


                        /*FileStream fileStr = new FileStream(@textBox1.Text, 
                FileMode.Create);

    

                BinaryWriter binWriter = new BinaryWriter(fileStr);
    
                int intArr = 500;
                int intArr = 500;
                binWriter.Write(intArr);
    
                binWriter.Close();
                binWriter2.Close();*/


                        // Datei öffnen und den Inhalt byteweise auslesen 
                        FileInfo fi = new FileInfo(@dateigroesse1);

                        //FileStream fs = new FileStream(@dateigroesse1, FileMode.Open);
                        FileStream fs = new FileStream(@dateigroesse1, FileMode.Open, FileAccess.Read);

                        //(Filestream)dlgOpenFile.OpenFile();
                        //BinaryReader fx = new BinaryReader(dateigroesse1);

                        //byte[] byteArr = new byte[fi.Length];
                        byte nurEinByte = 0;
                        //fs.Unlock(0,fi.Length);



                        byte falsche_datei = 0;
                        long dateiLaenge = 0;
                        long dateiLaenge2 = 0;
                        long dateiLaenge3 = 0;
                        long dateiLaenge4 = 0;
                        long dateiLaenge5 = 0;

                        if (richtung_info == 0)
                        { dateiLaenge = fi.Length; }
                        else
                        {
                            if (fi.Length > 70)
                            {
                                dateiLaenge = fi.Length - 70; //nur verschl. Code
                            }
                            else
                            {
                                dateiLaenge = 0;
                            }
                        }
                        dateiLaenge5 = dateiLaenge + 70; // Komplett


                        dateiLaenge4 = dateiLaenge5 - 0x36; //ohne BMP-Kopf
                        dateiLaenge3 = dateiLaenge4 / 3; //Anzahl an Pixel (24Bit pro Pixel)
                        dateiLaenge2 = (long)Math.Sqrt((double)(dateiLaenge3));
                        //Hier wird die Dateilaenge ermittelt
                        //Hier kann der BMP-Rahmen gelesen und geschrieben werden

                        //BMP-Rahmen Start
                        if (richtung_info == 1)
                        {
                            //dummy_byte = (byte)fs.ReadByte();
                            //dummy_byte2 = (byte)fs.ReadByte();

                            for (long s = 0; s < 43; s++)/*Lese die ersten 43 BMP Bytes der verschlüsselten Datei*/
                            {
                                dummy_byte1 = (byte)fs.ReadByte();
                            }

                            dummy_byte1 = (byte)fs.ReadByte(); //Lese TURBINE um zu prüfen, ob mit Turbine verschlüsselt wurde
                            dummy_byte2 = (byte)fs.ReadByte();
                            dummy_byte3 = (byte)fs.ReadByte();
                            dummy_byte4 = (byte)fs.ReadByte();
                            dummy_byte5 = (byte)fs.ReadByte();
                            dummy_byte6 = (byte)fs.ReadByte();
                            dummy_byte7 = (byte)fs.ReadByte();

                            dummy_byte8 = (byte)fs.ReadByte(); //enthält den ehemaligen suffix
                            dummy_byte9 = (byte)fs.ReadByte();
                            dummy_byte10 = (byte)fs.ReadByte();
                            dummy_byte11 = (byte)fs.ReadByte();

                            Turbine_Name[0] = dummy_byte1;
                            Turbine_Name[1] = dummy_byte2;
                            Turbine_Name[2] = dummy_byte3;
                            Turbine_Name[3] = dummy_byte4;
                            Turbine_Name[4] = dummy_byte5;
                            Turbine_Name[5] = dummy_byte6;
                            Turbine_Name[6] = dummy_byte7;

                            Turbine_Typ_Endung[0] = dummy_byte8;
                            Turbine_Typ_Endung[1] = dummy_byte9;
                            Turbine_Typ_Endung[2] = dummy_byte10;
                            Turbine_Typ_Endung[3] = dummy_byte11;
                          
                      

                            Turbine_Header = ByteArrayToString(Turbine_Name);
                            Turbine_Typ = ByteArrayToString(Turbine_Typ_Endung); 


                            


                            if ((dummy_byte1 == 'T') && (dummy_byte2 == 'U') && (dummy_byte3 == 'R') && (dummy_byte4 == 'B') && (dummy_byte5 == 'I') &&
                                (dummy_byte6 == 'N') && (dummy_byte7 == 'E'))//Ist TURBINE im BMP Header enthalten?
                            {
                                ShowFg(Turbine_Typ, "After decryption the File Type is:");
                            }
                            /*else 
                            {
                                dateiLaenge = 0;
                                ShowFg("Decryption not possible. File is not a encrypted Turbine-file.");
                                ShowFg(Turbine_Header, "Wrong Header Information");
                                radioButton3.IsChecked = false;
                                radioButton3.Foreground = new SolidColorBrush(Colors.Gray);
                                radioButton3.FontWeight = FontWeights.Normal;
                                
                                radioButton4.IsChecked = true;
                                radioButton4.Foreground = new SolidColorBrush(Colors.Black);
                                radioButton4.FontWeight = FontWeights.Heavy;

                             
                                

                                falsche_datei = 1;
                            }*/ 


                     

                        }

                        else
                        {
                            //Schreibe BMP-Rahmen

                            /*424D3056220000000000360000002800
                              0000F3010000F3010000010018000000
                              00000000000000000000000000000000
                           */

                            binWriter2.Write((byte)(0x42));
                            binWriter2.Write((byte)(0x4D));
                            binWriter2.Write((byte)(dateiLaenge5));
                            binWriter2.Write((byte)(dateiLaenge5 >> 8));
                            binWriter2.Write((byte)(dateiLaenge5 >> 16));
                            binWriter2.Write((byte)(dateiLaenge5 >> 24));
                            // Position 6 = Turbine-Versions-Byte:
                            //   0x00 = Legacy V1, 0x01 = V2 PBKDF2, 0x02 = V2 Key-File
                            //   0x03 = V3 Key-File+Whitening, 0x04 = V4 PBKDF2+Fix, 0x05 = V4 Key-File+Fix
                            //   0x06 = V5.2 PBKDF2+Fix+ImprovedPwInfo, 0x07 = V5.2 Key-File+Fix+ImprovedPwInfo
                            binWriter2.Write(version_byte_to_write);
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x36));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x28));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(dateiLaenge2));
                            binWriter2.Write((byte)(dateiLaenge2 >> 8));
                            binWriter2.Write((byte)(dateiLaenge2 >> 16));
                            binWriter2.Write((byte)(dateiLaenge2 >> 24));
                            binWriter2.Write((byte)(dateiLaenge2));
                            binWriter2.Write((byte)(dateiLaenge2 >> 8));
                            binWriter2.Write((byte)(dateiLaenge2 >> 16));
                            binWriter2.Write((byte)(dateiLaenge2 >> 24));
                            binWriter2.Write((byte)(0x1));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x18));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(dateiLaenge4));
                            binWriter2.Write((byte)(dateiLaenge4 >> 8));
                            binWriter2.Write((byte)(dateiLaenge4 >> 16));
                            binWriter2.Write((byte)(dateiLaenge4 >> 24));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)(0x0));
                            //binWriter2.Write((byte)(0x0));
                            binWriter2.Write((byte)('T'));
                            binWriter2.Write((byte)('U'));
                            binWriter2.Write((byte)('R'));
                            binWriter2.Write((byte)('B'));
                            binWriter2.Write((byte)('I'));
                            binWriter2.Write((byte)('N'));
                            binWriter2.Write((byte)('E'));
                            binWriter2.Write((byte)(datei_endung2[0])); //Speichert Datei-Endung(suffix)
                            binWriter2.Write((byte)(datei_endung2[1])); //Speichert Datei-Endung(suffix)
                            binWriter2.Write((byte)(datei_endung2[2])); //Speichert Datei-Endung(suffix)

                            if (datei_endung_info3 == 3)
                            {
                                binWriter2.Write((byte)(0x0));//(datei_endung2[3])); //Speichert Datei-Endung(suffix)
                            }

                            if (datei_endung_info3 == 4)
                            {
                                binWriter2.Write((byte)(datei_endung2[3])); //Speichert Datei-Endung(suffix)
                            }


                        }
                        //BMP-Rahmen Ende



                        for (long s = 0; s < 16; s++)
                        {
                            if (richtung_info == 1)
                            {
                                zufall[s] = (byte)fs.ReadByte();
                            }
                            else
                            {
                                binWriter2.Write((byte)(zufall[s]));
                            }

                            if (s == 15)
                            {
                                gear_a[2] = (byte)(gear_a[2] ^ zufall[0]);
                                gear_a2[1] = (byte)(gear_a2[1] ^ zufall[1]);
                                gear_a3[5] = (byte)(gear_a3[5] ^ zufall[2]);
                                gear_a4[7] = (byte)(gear_a4[7] ^ zufall[3]);
                                gear_b[7] = (byte)(gear_b[7] ^ zufall[4]);
                                gear_b2[3] = (byte)(gear_b2[3] ^ zufall[5]);
                                gear_b3[5] = (byte)(gear_b3[5] ^ zufall[6]);
                                gear_b4[9] = (byte)(gear_b4[9] ^ zufall[7]);
                                gear_a3[10] = (byte)(gear_a3[10] ^ zufall[8]);
                                gear_a3[11] = (byte)(gear_a3[11] ^ zufall[9]);
                                gear_a3[12] = (byte)(gear_a3[12] ^ zufall[10]);
                                gear_a4[13] = (byte)(gear_a4[13] ^ zufall[11]);
                                gear_a[14] = (byte)(gear_a[14] ^ zufall[12]);
                                gear_a[15] = (byte)(gear_a[15] ^ zufall[13]);
                                gear_a2[13] = (byte)(gear_a2[13] ^ zufall[14]);
                                gear_a2[14] = (byte)(gear_a2[14] ^ zufall[15]);

                            }
                        }
                        // Datenstrom in ein Byte-Array einlesen 
                        // fs.Read(byteArr, 0, (int)fi.Length);

                        // MessageBox.Show("Prozent = " + fi.Length);

                        //Console.Write("Interpretation als Byte-Array: "); 
                        for (long i = 0; i < dateiLaenge; i++)
                        //Console.Write(byteArr[i] + " "); 
                        //Console.Write("\n\n"); 
                        {
                            nurEinByte = (byte)fs.ReadByte();


                            /*Verschlüsslungbytes generieren*/

                            /*_________________________________________________________________________*/

                            /*~A:70*/
                            /*~+:a_verknuepfungen*/
                            /*~T*/
                            gear_ergebnisa1 = (byte)((gear_a[0] ^ gear_a[5]) ^ gear_b[0]);
                            gear_ergebnisa2 = (byte)((gear_a[1] ^ gear_a[6]) ^ gear_b[1]);
                            gear_ergebnisa3 = (byte)((gear_a[2] ^ gear_a[gear_ergebnisd2_2 & 0x0F]) ^ gear_b[2]);

                            gear_ergebnisa4 = (byte)((gear_a[3] ^ gear_a[8]) ^ gear_b[3]);

                            gear_ergebnisa5 = (byte)((gear_a[4] ^ gear_a[9]) ^ (gear_b[4] + gear_ergebnisd1));


                            gear_ergebnisa6 = (byte)((gear_a[6] ^ gear_a[13]) ^ gear_b[5]);
                            gear_ergebnisa7 = (byte)((gear_a[(gear_ergebnise1 & 0x0F)] ^ gear_a[14]) ^ gear_b[6]);
                            gear_ergebnisa8 = (byte)((gear_a[8] ^ gear_a[15]) ^ gear_b[7]);
                            gear_ergebnisa9 = (byte)((gear_a[9] ^ gear_a[16]) ^ gear_b[8]);
                            gear_ergebnisa10 = (byte)((gear_a[10] ^ gear_a[17]) ^ gear_b[9]);


                            gear_ergebnisa1_2 = (byte)((gear_a2[0] ^ gear_a2[5]) ^ gear_b2[0]);
                            gear_ergebnisa2_2 = (byte)((gear_a2[1] ^ gear_a2[6]) ^ gear_b2[1]);
                            gear_ergebnisa3_2 = (byte)((gear_a2[2] ^ gear_a2[gear_ergebnisd2 & 0x0F]) ^ gear_b2[2]);

                            gear_ergebnisa4_2 = (byte)((gear_a2[3] ^ gear_a2[8]) ^ gear_b2[3]);

                            gear_ergebnisa5_2 = (byte)((gear_a2[4] ^ gear_a2[9]) ^ (gear_b2[4] + gear_ergebnisd1_2));


                            gear_ergebnisa6_2 = (byte)((gear_a2[6] ^ gear_a2[13]) ^ gear_b2[5]);
                            gear_ergebnisa7_2 = (byte)((gear_a2[(gear_ergebnise1_2 & 0x0F)] ^ gear_a2[14]) ^ gear_b2[6]);
                            gear_ergebnisa8_2 = (byte)((gear_a2[8] ^ gear_a2[15]) ^ gear_b2[7]);
                            gear_ergebnisa9_2 = (byte)((gear_a2[9] ^ gear_a2[16]) ^ gear_b2[8]);
                            gear_ergebnisa10_2 = (byte)((gear_a2[10] ^ gear_a2[17]) ^ gear_b2[9]);


                            gear_ergebnisa1_3 = (byte)((gear_a3[0] ^ gear_a3[5]) ^ gear_b3[0]);
                            gear_ergebnisa2_3 = (byte)((gear_a3[1] ^ gear_a3[6]) ^ gear_b3[1]);
                            gear_ergebnisa3_3 = (byte)((gear_a3[2] ^ gear_a3[gear_ergebnisd2_3 & 0x0F]) ^ gear_b3[2]);

                            gear_ergebnisa4_3 = (byte)((gear_a3[3] ^ gear_a3[8]) ^ gear_b3[3]);

                            gear_ergebnisa5_3 = (byte)((gear_a3[4] ^ gear_a3[9]) ^ (gear_b3[4] + gear_ergebnisd1_3));


                            gear_ergebnisa6_3 = (byte)((gear_a3[6] ^ gear_a3[13]) ^ gear_b3[5]);
                            gear_ergebnisa7_3 = (byte)((gear_a3[(gear_ergebnise1_3 & 0x0F)] ^ gear_a3[14]) ^ gear_b3[6]);
                            gear_ergebnisa8_3 = (byte)((gear_a3[8] ^ gear_a3[15]) ^ gear_b3[7]);
                            gear_ergebnisa9_3 = (byte)((gear_a3[9] ^ gear_a3[16]) ^ gear_b3[8]);
                            gear_ergebnisa10_3 = (byte)((gear_a3[10] ^ gear_a3[17]) ^ gear_b3[9]);



                            gear_ergebnisa1_4 = (byte)((gear_a4[0] ^ gear_a4[5]) ^ gear_b4[0]);
                            gear_ergebnisa2_4 = (byte)((gear_a4[1] ^ gear_a4[6]) ^ gear_b4[1]);
                            gear_ergebnisa3_4 = (byte)((gear_a4[2] ^ gear_a4[gear_ergebnisd1_4 & 0x0F]) ^ gear_b4[2]);

                            gear_ergebnisa4_4 = (byte)((gear_a4[3] ^ gear_a4[8]) ^ gear_b4[3]);

                            gear_ergebnisa5_4 = (byte)((gear_a4[4] ^ gear_a4[9]) ^ (gear_b4[4] + gear_ergebnisd1_4));


                            gear_ergebnisa6_4 = (byte)((gear_a4[6] ^ gear_a4[13]) ^ gear_b4[5]);
                            gear_ergebnisa7_4 = (byte)((gear_a4[(gear_ergebnise1_4 & 0x0F)] ^ gear_a4[14]) ^ gear_b4[6]);
                            gear_ergebnisa8_4 = (byte)((gear_a4[8] ^ gear_a4[15]) ^ gear_b4[7]);
                            gear_ergebnisa9_4 = (byte)((gear_a4[9] ^ gear_a4[16]) ^ gear_b4[8]);
                            gear_ergebnisa10_4 = (byte)((gear_a4[10] ^ gear_a4[17]) ^ gear_b4[9]);

                            /*~E:A70*/
                            /*~A:71*/
                            /*~+:b_verknuepfungen*/
                            /*~T*/
                            gear_ergebnisb1 = (byte)((gear_ergebnisa1 ^ gear_ergebnisa2) ^ gear_c[0]);
                            gear_ergebnisb2 = (byte)((gear_b[10] ^ gear_a[11] ^ gear_ergebnisa3) ^ gear_c[1]);
                            gear_ergebnisb3 = (byte)((gear_ergebnisa3 ^ gear_ergebnisa4) ^ gear_c[2]);
                            gear_ergebnisb4 = (byte)((gear_b[11] ^ gear_a[12] ^ gear_ergebnisa5) ^ gear_c[3]);


                            gear_ergebnisb5 = (byte)((gear_ergebnisa6 ^ gear_ergebnisa7) ^ gear_c[4]);
                            gear_ergebnisb6 = (byte)((gear_b[12] ^ gear_ergebnisa8) ^ gear_c[5]);
                            gear_ergebnisb7 = (byte)((gear_ergebnisa8 ^ gear_ergebnisa9) ^ gear_c[6]);
                            gear_ergebnisb8 = (byte)((gear_b[13] ^ gear_ergebnisa10) ^ gear_c[7]);


                            gear_ergebnisb1_2 = (byte)((gear_ergebnisa1_2 ^ gear_ergebnisa2_2) ^ gear_c2[0]);
                            gear_ergebnisb2_2 = (byte)((gear_b2[10] ^ gear_a2[11] ^ gear_ergebnisa3_2) ^ gear_c2[1]);
                            gear_ergebnisb3_2 = (byte)((gear_ergebnisa3_2 ^ gear_ergebnisa4_2) ^ gear_c2[2]);
                            gear_ergebnisb4_2 = (byte)((gear_b2[11] ^ gear_a2[12] ^ gear_ergebnisa5_2) ^ gear_c2[3]);


                            gear_ergebnisb5_2 = (byte)((gear_ergebnisa6_2 ^ gear_ergebnisa7_2) ^ gear_c2[4]);
                            gear_ergebnisb6_2 = (byte)((gear_b2[12] ^ gear_ergebnisa8_2) ^ gear_c2[5]);
                            gear_ergebnisb7_2 = (byte)((gear_ergebnisa8_2 ^ gear_ergebnisa9_2) ^ gear_c2[6]);
                            gear_ergebnisb8_2 = (byte)((gear_b2[13] ^ gear_ergebnisa10_2) ^ gear_c2[7]);

                            gear_ergebnisb1_3 = (byte)((gear_ergebnisa1_3 ^ gear_ergebnisa2_3) ^ gear_c3[0]);
                            gear_ergebnisb2_3 = (byte)((gear_b3[10] ^ gear_a3[11] ^ gear_ergebnisa3_3) ^ gear_c3[1]);
                            gear_ergebnisb3_3 = (byte)((gear_ergebnisa3_3 ^ gear_ergebnisa4_3) ^ gear_c3[2]);
                            gear_ergebnisb4_3 = (byte)((gear_b3[11] ^ gear_a3[12] ^ gear_ergebnisa5_3) ^ gear_c3[3]);


                            gear_ergebnisb5_3 = (byte)((gear_ergebnisa6_3 ^ gear_ergebnisa7_3) ^ gear_c3[4]);
                            gear_ergebnisb6_3 = (byte)((gear_b3[12] ^ gear_ergebnisa8_3) ^ gear_c3[5]);
                            gear_ergebnisb7_3 = (byte)((gear_ergebnisa8_3 ^ gear_ergebnisa9_3) ^ gear_c3[6]);
                            gear_ergebnisb8_3 = (byte)((gear_b3[13] ^ gear_ergebnisa10_3) ^ gear_c3[7]);


                            gear_ergebnisb1_4 = (byte)((gear_ergebnisa1_4 ^ gear_ergebnisa2_4) ^ gear_c4[0]);
                            gear_ergebnisb2_4 = (byte)((gear_b4[10] ^ gear_a4[11] ^ gear_ergebnisa3_4) ^ gear_c4[1]);
                            gear_ergebnisb3_4 = (byte)((gear_ergebnisa3_4 ^ gear_ergebnisa4_4) ^ gear_c4[2]);
                            gear_ergebnisb4_4 = (byte)((gear_b4[11] ^ gear_a4[12] ^ gear_ergebnisa5_4) ^ gear_c4[3]);


                            gear_ergebnisb5_4 = (byte)((gear_ergebnisa6_4 ^ gear_ergebnisa7_4) ^ gear_c4[4]);
                            gear_ergebnisb6_4 = (byte)((gear_b4[12] ^ gear_ergebnisa8_4) ^ gear_c4[5]);
                            gear_ergebnisb7_4 = (byte)((gear_ergebnisa8_4 ^ gear_ergebnisa9_4) ^ gear_c4[6]);
                            gear_ergebnisb8_4 = (byte)((gear_b4[13] ^ gear_ergebnisa10_4) ^ gear_c4[7]);

                            /*~E:A71*/
                            /*~A:72*/
                            /*~+:c_verknuepfungen*/
                            /*~T*/
                            gear_ergebnisc1 = (byte)(gear_ergebnisb1 ^ gear_ergebnisb2);
                            gear_ergebnisc2 = (byte)(gear_ergebnisb3 ^ gear_ergebnisb4);

                            gear_ergebnisc3 = (byte)(gear_ergebnisb5 ^ gear_ergebnisb6);
                            gear_ergebnisc4 = (byte)(gear_ergebnisb7 ^ gear_ergebnisb8);


                            gear_ergebnisc1_2 = (byte)(gear_ergebnisb1_2 ^ gear_ergebnisb2_2);
                            gear_ergebnisc2_2 = (byte)(gear_ergebnisb3_2 ^ gear_ergebnisb4_2);

                            gear_ergebnisc3_2 = (byte)(gear_ergebnisb5_2 ^ gear_ergebnisb6_2);
                            gear_ergebnisc4_2 = (byte)(gear_ergebnisb7_2 ^ gear_ergebnisb8_2);


                            gear_ergebnisc1_3 = (byte)(gear_ergebnisb1_3 ^ gear_ergebnisb2_3);
                            gear_ergebnisc2_3 = (byte)(gear_ergebnisb3_3 ^ gear_ergebnisb4_3);

                            gear_ergebnisc3_3 = (byte)(gear_ergebnisb5_3 ^ gear_ergebnisb6_3);
                            gear_ergebnisc4_3 = (byte)(gear_ergebnisb7_3 ^ gear_ergebnisb8_3);


                            gear_ergebnisc1_4 = (byte)(gear_ergebnisb1_4 ^ gear_ergebnisb2_4);
                            gear_ergebnisc2_4 = (byte)(gear_ergebnisb3_4 ^ gear_ergebnisb4_4);

                            gear_ergebnisc3_4 = (byte)(gear_ergebnisb5_4 ^ gear_ergebnisb6_4);
                            gear_ergebnisc4_4 = (byte)(gear_ergebnisb7_4 ^ gear_ergebnisb8_4);



                            /*~+:Endergebnisse*/
                            /*~T*/
                            gear_ergebnisd1 = (byte)(gear_ergebnisc1 ^ gear_ergebnisc2);

                            gear_ergebnisd2 = (byte)(gear_ergebnisc3 ^ gear_ergebnisc4);

                            gear_ergebnise1 = (byte)(gear_ergebnisd1 ^ gear_ergebnisd2);


                            gear_ergebnisd1_2 = (byte)(gear_ergebnisc1_2 ^ gear_ergebnisc2_2);

                            gear_ergebnisd2_2 = (byte)(gear_ergebnisc3_2 ^ gear_ergebnisc4_2);

                            gear_ergebnise1_2 = (byte)(gear_ergebnisd1_2 ^ gear_ergebnisd2_2);


                            gear_ergebnisd1_3 = (byte)(gear_ergebnisc1_3 ^ gear_ergebnisc2_3);

                            gear_ergebnisd2_3 = (byte)(gear_ergebnisc3_3 ^ gear_ergebnisc4_3);

                            gear_ergebnise1_3 = (byte)(gear_ergebnisd1_3 ^ gear_ergebnisd2_3);



                            gear_ergebnisd1_4 = (byte)(gear_ergebnisc1_4 ^ gear_ergebnisc2_4);

                            gear_ergebnisd2_4 = (byte)(gear_ergebnisc3_4 ^ gear_ergebnisc4_4);

                            gear_ergebnise1_4 = (byte)(gear_ergebnisd1_4 ^ gear_ergebnisd2_4);



                            gear_ergebnise1 = (byte)(gear_ergebnise1 ^ gear_ergebnise1_2 ^ gear_ergebnise1_3 ^ gear_ergebnise1_4);

                            // V4: S-Box Post-Whitening — nichtlineare Substitution NUR fuer die Ausgabe.
                            // gear_ergebnise1 bleibt unveraendert fuer Gear-Feedback und Rotation.
                            // gear_ergebnise1_out wird fuer XOR mit Plaintext verwendet.
                            byte gear_ergebnise1_out = use_fixed_masks ? TURBINE_SBOX[gear_ergebnise1] : gear_ergebnise1;

                            /*Endergebnis*/


                            /*
                            gear_ergebnise1_alt5= (byte) (gear_ergebnise1_alt4);
                            gear_ergebnise1_alt4= (byte) (gear_ergebnise1_alt3);
                            gear_ergebnise1_alt3= (byte) (gear_ergebnise1_alt2);
                            gear_ergebnise1_alt2= (byte) (gear_ergebnise1_alt1);
                            gear_ergebnise1_alt1= (byte) (gear_ergebnise1);
                            gear_ergebnise1= (byte) (((gear_ergebnise1^gear_ergebnise1_2)+(gear_ergebnise1_3^gear_ergebnise1_4))^
                            ((gear_ergebnise1_alt2+gear_ergebnise1_alt1)^gear_ergebnise1_alt3+gear_ergebnise1_alt4^gear_ergebnise1_alt5));*/



                            /*~E:A73*/
                            /*~E:A69*/
                            /*~A:74*/
                            /*~+:A-R„der*/
                            /*~I:75*/
                            if (takt < 9)
                            /*~-1*/
                            {
                                /*~T*/

                                /*RAD 1*/
                                temp_gear_a2 = (gear_a[0]);

                                /*~I:76*/
                                if (((gear_ergebnise1 & 0x1) == 1))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[0] = (byte)(gear_a[1]);
                                    gear_a2[0] = (byte)((0xFF ^ gear_a2[1]));
                                    gear_a3[0] = (byte)((0xFF ^ gear_a3[1]));
                                    gear_a4[0] = (byte)(gear_a4[1]);


                                    /*~-1*/
                                }
                                /*~O:I76*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[0] = (byte)((0xFF ^ gear_a[1]));
                                    gear_a2[0] = (byte)(gear_a2[1]);
                                    gear_a4[0] = (byte)((0xFF ^ gear_a4[1]));
                                    gear_a3[0] = (byte)(gear_a3[1]);

                                    /*~-1*/
                                }
                                /*~E:I76*/
                                /*~T*/

                                gear_a[1] = (byte)(gear_a[2]);
                                gear_a2[1] = (byte)(gear_a2[2]);
                                gear_a3[1] = (byte)(gear_a3[2]);
                                gear_a4[1] = (byte)(gear_a4[2]);




                                /*~I:77*/
                                if (((gear_ergebnise1 & 0x2) == 2))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[2] = (byte)(gear_a[3]);
                                    gear_a2[2] = (byte)((0xFF ^ gear_a2[3]));
                                    gear_a3[2] = (byte)((0xFF ^ gear_a3[3]));
                                    gear_a4[2] = (byte)((0xFF ^ gear_a4[3]));


                                    /*~-1*/
                                }
                                /*~O:I77*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[2] = (byte)((0xFF ^ gear_a[3]));
                                    gear_a2[2] = (byte)(gear_a2[3]);
                                    gear_a3[2] = (byte)(gear_a3[3]);
                                    gear_a4[2] = (byte)(gear_a4[3]);


                                    /*~-1*/
                                }
                                /*~E:I77*/
                                /*~T*/
                                gear_a[3] = (byte)(gear_a[4]);
                                gear_a[4] = (byte)(gear_a[5]);

                                gear_a2[3] = (byte)(gear_a2[4]);
                                gear_a2[4] = (byte)(gear_a2[5]);


                                gear_a3[3] = (byte)(gear_a3[4]);
                                gear_a3[4] = (byte)(gear_a3[5]);


                                gear_a4[3] = (byte)(gear_a4[4]);
                                gear_a4[4] = (byte)(gear_a4[5]);



                                /*~I:78*/
                                if (((gear_ergebnise1 & 0x4) == 4))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[5] = (byte)(gear_a[6]);
                                    gear_a2[5] = (byte)((0xFF ^ gear_a2[6]));
                                    gear_a3[5] = (byte)(gear_a3[6]);
                                    gear_a4[5] = (byte)(gear_a4[6]);


                                    /*~-1*/
                                }
                                /*~O:I78*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[5] = (byte)((0xFF ^ gear_a[6]));
                                    gear_a2[5] = (byte)(gear_a2[6]);
                                    gear_a3[5] = (byte)((0xFF ^ gear_a3[6]));
                                    gear_a4[5] = (byte)((0xFF ^ gear_a4[6]));


                                    /*~-1*/
                                }
                                /*~E:I78*/
                                /*~T*/

                                gear_a[6] = (byte)(gear_a[7]);
                                gear_a2[6] = (byte)(gear_a2[7]);

                                gear_a3[6] = (byte)(gear_a3[7]);
                                gear_a4[6] = (byte)(gear_a4[7]);




                                /*~I:79*/
                                if (((gear_ergebnise1 & 0x8) == 8))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[7] = (byte)(gear_a[8]);
                                    gear_a2[7] = (byte)((0xFF ^ gear_a2[8]));
                                    gear_a3[7] = (byte)(gear_a3[8]);
                                    gear_a4[7] = (byte)((0xFF ^ gear_a4[8]));

                                    /*~-1*/
                                }
                                /*~O:I79*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[7] = (byte)((0xFF ^ gear_a[8]));
                                    gear_a2[7] = (byte)(gear_a2[8]);
                                    gear_a3[7] = (byte)((0xFF ^ gear_a3[8]));
                                    gear_a4[7] = (byte)(gear_a4[8]);


                                    /*~-1*/
                                }
                                /*~E:I79*/
                                /*~T*/
                                gear_a[8] = (byte)(gear_a2[9]);
                                gear_a2[8] = (byte)(gear_a[9]);

                                gear_a3[8] = (byte)(gear_a3[9]);

                                gear_a4[8] = (byte)(gear_a4[9]);





                                /*~I:80*/
                                if (((gear_ergebnise1 & 0x1) == 1))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[9] = (byte)(gear_a[10]);
                                    gear_a2[9] = (byte)((0xFF ^ gear_a2[10]));
                                    gear_a3[9] = (byte)(gear_a3[10]);
                                    gear_a4[9] = (byte)(gear_a4[10]);


                                    /*~-1*/
                                }
                                /*~O:I80*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[9] = (byte)((0xFF ^ gear_a[10]));
                                    gear_a2[9] = (byte)(gear_a2[10]);
                                    gear_a3[9] = (byte)((0xFF ^ gear_a3[10]));
                                    gear_a4[9] = (byte)((0xFF ^ gear_a4[10]));


                                    /*~-1*/
                                }
                                /*~E:I80*/
                                /*~T*/

                                gear_a[10] = (byte)(gear_a[11]);
                                gear_a2[10] = (byte)(gear_a2[11]);
                                gear_a3[10] = (byte)(gear_a3[11]);
                                gear_a4[10] = (byte)(gear_a4[11]);



                                /*~I:81*/
                                if (((gear_ergebnise1_4 & 0x2) == 2))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[11] = (byte)(gear_a[12]);
                                    gear_a2[11] = (byte)((0xFF ^ gear_a2[12]));
                                    gear_a3[11] = (byte)((0xFF ^ gear_a3[12]));
                                    gear_a4[11] = (byte)(gear_a4[12]);

                                    /*~-1*/
                                }
                                /*~O:I81*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[11] = (byte)((0xFF ^ gear_a[12]));
                                    gear_a2[11] = (byte)(gear_a2[12]);
                                    gear_a3[11] = (byte)(gear_a3[12]);
                                    gear_a4[11] = (byte)((0xFF ^ gear_a4[12]));


                                    /*~-1*/
                                }
                                /*~E:I81*/
                                /*~T*/
                                gear_a[12] = (byte)(gear_a[13]);
                                gear_a[13] = (byte)(gear_a[14]);

                                gear_a2[12] = (byte)(gear_a2[13]);
                                gear_a2[13] = (byte)(gear_a2[14]);

                                gear_a3[12] = (byte)(gear_a3[13]);
                                gear_a3[13] = (byte)(gear_a3[14]);


                                gear_a4[12] = (byte)(gear_a4[13]);
                                gear_a4[13] = (byte)(gear_a4[14]);



                                /*~I:82*/
                                if (((gear_ergebnise1_3 & 0x4) == 4))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[14] = (byte)(gear_a[15]);
                                    gear_a2[14] = (byte)((0xFF ^ gear_a2[15]));
                                    gear_a3[14] = (byte)((0xFF ^ gear_a3[15]));
                                    gear_a4[14] = (byte)((0xFF ^ gear_a4[15]));


                                    /*~-1*/
                                }
                                /*~O:I82*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[14] = (byte)((0xFF ^ gear_a[15]));
                                    gear_a2[14] = (byte)(gear_a2[15]);
                                    gear_a3[14] = (byte)(gear_a3[15]);
                                    gear_a4[14] = (byte)(gear_a4[15]);



                                    /*~-1*/
                                }
                                /*~E:I82*/
                                /*~T*/

                                gear_a[15] = (byte)(gear_a[16]);
                                gear_a2[15] = (byte)(gear_a2[16]);
                                gear_a3[15] = (byte)(gear_a3[16]);
                                gear_a4[15] = (byte)(gear_a4[16]);



                                /*~I:83*/
                                // V4-Fix (2026-06-09): Die Masken 0x55/0xAA erzeugten einen LSB-Bias
                                // (+0.21%, Z=3.29, NIST Monobit FAIL), weil 0xAA Bit 0 nicht flippt und
                                // das LSB in gear_a3/a4 stagniert. Ab V4 werden die Masken aus den
                                // Variablen mask_a3_16/mask_a4_16 gelesen (0xFF fuer V4, 0x55/0xAA fuer V1-V3).
                                if (((gear_ergebnise1 & 0x8) == 8))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_a[16] = (byte)(gear_a[17]);
                                    gear_a2[16] = (byte)((0xFF ^ gear_a2[17]));
                                    gear_a3[16] = (byte)(gear_a3[17]);
                                    gear_a4[16] = (byte)(gear_a4[17]);


                                    /*~-1*/
                                }
                                /*~O:I83*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_a[16] = (byte)((0xFF ^ gear_a[17]));
                                    gear_a2[16] = (byte)(gear_a2[17]);
                                    gear_a3[16] = (byte)((mask_a3_16 ^ gear_a3[17])); // V4: 0xFF (fix), V1-V3: 0x55 (LSB-Bias)
                                    gear_a4[16] = (byte)((mask_a4_16 ^ gear_a4[17])); // V4: 0xFF (fix), V1-V3: 0xAA (LSB-Bias)


                                    /*~-1*/
                                }
                                /*~E:I83*/
                                /*~T*/
                                gear_a[17] = use_fixed_masks ? (byte)(gear_a[17] ^ gear_ergebnise1) : (byte)(gear_a[17] + gear_ergebnise1); // V4: XOR, V1-V3: ADD
                                gear_a2[17] = (byte)(gear_a2[17] ^ gear_ergebnise1_2);
                                gear_a3[17] = use_fixed_masks ? (byte)(gear_a3[17] ^ gear_ergebnise1_3) : (byte)(gear_a3[17] + gear_ergebnise1_3); // V4: XOR, V1-V3: ADD
                                gear_a4[17] = (byte)(gear_a4[17] ^ gear_ergebnise1_4);




                                /*~-1*/
                            }
                            /*~E:I75*/
                            /*~T*/


                            /*~E:A74*/
                            /*~A:84*/
                            /*~+:B-R„der*/
                            /*~I:85*/
                            if ((takt >= 9) && (takt < 16))
                            /*~-1*/
                            {
                                /*~T*/



                                /*RAD 2*/

                                gear_b[13] = (byte)(gear_b[12]);
                                gear_b[12] = (byte)(gear_b[11]);

                                gear_b2[13] = (byte)(gear_b2[12]);
                                gear_b2[12] = (byte)(gear_b2[11]);

                                gear_b3[13] = (byte)(gear_b3[12]);
                                gear_b3[12] = (byte)(gear_b3[11]);

                                gear_b4[13] = (byte)(gear_b4[12]);
                                gear_b4[12] = (byte)(gear_b4[11]);



                                /*~I:86*/
                                if (((gear_ergebnise1 & 0x10) == 0x10))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_b[11] = (byte)(gear_b[10]);
                                    gear_b2[11] = (byte)((0xFF ^ gear_b2[10]));
                                    gear_b3[11] = (byte)(gear_b3[10]);
                                    gear_b4[11] = (byte)((0xFF ^ gear_b4[10]));



                                    /*~-1*/
                                }
                                /*~O:I86*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_b[11] = (byte)((0xFF ^ gear_b[10]));
                                    gear_b2[11] = (byte)(gear_b2[10]);
                                    gear_b3[11] = (byte)((0xFF ^ gear_b3[10]));
                                    gear_b4[11] = (byte)(gear_b4[10]);

                                    /*~-1*/
                                }
                                /*~E:I86*/
                                /*~T*/

                                gear_b[10] = (byte)(gear_b[9]);
                                gear_b2[10] = (byte)(gear_b2[9]);

                                gear_b3[10] = (byte)(gear_b3[9]);
                                gear_b4[10] = (byte)(gear_b4[9]);




                                /*~I:87*/
                                if (((gear_ergebnise1 & 0x20) == 0x20))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_b[9] = (byte)(gear_b[8]);
                                    gear_b2[9] = (byte)((0xFF ^ gear_b2[8]));
                                    gear_b3[9] = (byte)(gear_b3[8]);
                                    gear_b4[9] = (byte)((0xFF ^ gear_b4[8]));

                                    /*~-1*/
                                }
                                /*~O:I87*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_b[9] = (byte)((0xFF ^ gear_b[8]));
                                    gear_b2[9] = (byte)(gear_b2[8]);
                                    gear_b3[9] = (byte)((0xFF ^ gear_b3[8]));
                                    gear_b4[9] = (byte)(gear_b4[8]);




                                    /*~-1*/
                                }
                                /*~E:I87*/
                                /*~T*/
                                gear_b[8] = (byte)(gear_b[7]);
                                gear_b2[8] = (byte)(gear_b2[7]);
                                gear_b3[8] = (byte)(gear_b3[7]);
                                gear_b4[8] = (byte)(gear_b4[7]);



                                /*~I:88*/
                                if (((gear_ergebnise1 & 0x40) == 0x40))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_b[7] = (byte)(gear_b[6]);
                                    gear_b2[7] = (byte)((0xFF ^ gear_b2[6]));
                                    gear_b3[7] = (byte)(gear_b3[6]);
                                    gear_b4[7] = (byte)((0xFF ^ gear_b4[6]));



                                    /*~-1*/
                                }
                                /*~O:I88*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_b[7] = (byte)((0xFF ^ gear_b[6]));
                                    gear_b2[7] = (byte)(gear_b2[6]);
                                    gear_b3[7] = (byte)((0xFF ^ gear_b3[6]));
                                    gear_b4[7] = (byte)(gear_b4[6]);

                                    /*~-1*/
                                }
                                /*~E:I88*/
                                /*~T*/
                                gear_b[6] = (byte)(gear_b[5]);
                                gear_b2[6] = (byte)(gear_b2[5]);

                                gear_b3[6] = (byte)(gear_b3[5]);
                                gear_b4[6] = (byte)(gear_b4[5]);



                                /*~T*/

                                /*~I:89*/
                                if (((gear_ergebnisd1 & 0x10) == 0x10))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_b[5] = (byte)(gear_b[4]);
                                    gear_b2[5] = (byte)((0xFF ^ gear_b2[4]));
                                    gear_b3[5] = (byte)(gear_b3[4]);
                                    gear_b4[5] = (byte)((0xFF ^ gear_b4[4]));



                                    /*~-1*/
                                }
                                /*~O:I89*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_b[5] = (byte)((0xFF ^ gear_b[4]));
                                    gear_b2[5] = (byte)(gear_b2[4]);
                                    gear_b3[5] = (byte)((0xFF ^ gear_b3[4]));
                                    gear_b4[5] = (byte)(gear_b4[4]);

                                    /*~-1*/
                                }
                                /*~E:I89*/
                                /*~T*/

                                gear_b[4] = (byte)(gear_b[3]);
                                gear_b2[4] = (byte)(gear_b2[3]);

                                gear_b3[4] = (byte)(gear_b3[3]);
                                gear_b4[4] = (byte)(gear_b4[3]);





                                /*~I:90*/
                                if (((gear_ergebnisd1 & 0x20) == 0x20))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_b[3] = (byte)(gear_b[2]);
                                    gear_b2[3] = (byte)((0xFF ^ gear_b2[2]));

                                    gear_b3[3] = (byte)(gear_b3[2]);
                                    gear_b4[3] = (byte)((0xFF ^ gear_b4[2]));



                                    /*~-1*/
                                }
                                /*~O:I90*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_b[3] = (byte)((0xFF ^ gear_b[2]));
                                    gear_b2[3] = (byte)(gear_b2[2]);

                                    gear_b3[3] = (byte)((0xFF ^ gear_b3[2]));
                                    gear_b4[3] = (byte)(gear_b4[2]);


                                    /*~-1*/
                                }
                                /*~E:I90*/
                                /*~T*/
                                gear_b[2] = (byte)(gear_b[1]);
                                gear_b2[2] = (byte)(gear_b2[1]);

                                gear_b3[2] = (byte)(gear_b3[1]);
                                gear_b4[2] = (byte)(gear_b4[1]);




                                /*~I:91*/
                                // V4-Fix (2026-06-09): Analog zum gear_a-Block — 0x55/0xAA verursachten
                                // LSB-Stagnation in gear_b3/b4. Ab V4 ueber mask_b4_1/mask_b3_1 korrigiert.
                                if (((gear_ergebnisd1 & 0x40) == 0x40))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_b[1] = (byte)(gear_b[0]);
                                    gear_b2[1] = (byte)((0xFF ^ gear_b2[0]));
                                    gear_b3[1] = (byte)(gear_b3[0]);
                                    gear_b4[1] = (byte)((mask_b4_1 ^ gear_b4[0])); // V4: 0xFF (fix), V1-V3: 0x55 (LSB-Bias)

                                    /*~-1*/
                                }
                                /*~O:I91*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_b[1] = (byte)((0xFF ^ gear_b[0]));
                                    gear_b2[1] = (byte)(gear_b2[0]);
                                    gear_b3[1] = (byte)((mask_b3_1 ^ gear_b3[0])); // V4: 0xFF (fix), V1-V3: 0xAA (LSB-Bias)
                                    gear_b4[1] = (byte)(gear_b4[0]);



                                    /*~-1*/
                                }
                                /*~E:I91*/
                                /*~T*/
                                gear_b[0] = (byte)(gear_b[0] ^ gear_ergebnise1);
                                gear_b2[0] = use_fixed_masks ? (byte)(gear_b2[0] ^ (gear_ergebnise1_2 ^ temp_gear_a2)) : (byte)(gear_b2[0] + (gear_ergebnise1_2 ^ temp_gear_a2)); // V4: XOR, V1-V3: ADD
                                gear_b3[0] = (byte)(gear_b3[0] ^ gear_ergebnise1_3);
                                gear_b2[0] = use_fixed_masks ? (byte)(gear_b2[0] ^ gear_ergebnise1_4) : (byte)(gear_b2[0] + gear_ergebnise1_4); // V4: XOR, V1-V3: ADD




                                /*~T*/










                                /*~-1*/
                            }
                            /*~E:I85*/
                            /*~T*/





                            /*~E:A84*/
                            /*~A:92*/
                            /*~+:C-R„der*/
                            /*~I:93*/
                            if ((takt >= 16))
                            /*~-1*/
                            {
                                /*~T*/
                                /*RAD 3*/


                                /*~T*/
                                gear_c[0] = (byte)(gear_c[1]);
                                gear_c2[0] = (byte)(gear_c2[1]);
                                gear_c3[0] = (byte)(gear_c3[1]);
                                gear_c4[0] = (byte)(gear_c4[1]);



                                /*~I:94*/
                                if (((gear_ergebnise1 & 0x80) == 0x80))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_c[1] = (byte)(gear_c[2]);
                                    gear_c2[1] = (byte)((0xFF ^ gear_c2[2]));
                                    gear_c3[1] = (byte)(gear_c3[2]);
                                    gear_c4[1] = (byte)((0xFF ^ gear_c4[2]));

                                    /*~-1*/
                                }
                                /*~O:I94*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_c[1] = (byte)((0xFF ^ gear_c[2]));
                                    gear_c2[1] = (byte)(gear_c2[2]);
                                    gear_c3[1] = (byte)((0xFF ^ gear_c3[2]));
                                    gear_c4[1] = (byte)(gear_c4[2]);



                                    /*~-1*/
                                }
                                /*~E:I94*/
                                /*~T*/

                                gear_c[3] = (byte)(gear_c[4]);
                                gear_c2[3] = (byte)(gear_c2[4]);

                                gear_c3[3] = (byte)(gear_c3[4]);
                                gear_c4[3] = (byte)(gear_c4[4]);



                                /*~T*/





                                /*~T*/
                                gear_c[4] = (byte)(gear_c[5]);
                                gear_c2[4] = (byte)(gear_c2[5]);
                                gear_c3[4] = (byte)(gear_c3[5]);
                                gear_c4[4] = (byte)(gear_c4[5]);



                                /*~I:95*/
                                if (((gear_ergebnisd1 & 0x80) == 0x80))
                                /*~-1*/
                                {
                                    /*~T*/
                                    gear_c[5] = (byte)(gear_c[6]);
                                    gear_c2[5] = (byte)((0xFF ^ gear_c2[6]));
                                    gear_c3[5] = (byte)(gear_c3[6]);
                                    gear_c4[5] = (byte)((0xFF ^ gear_c4[6]));

                                    /*~-1*/
                                }
                                /*~O:I95*/
                                /*~-2*/
                                else
                                {
                                    /*~T*/
                                    gear_c[5] = (byte)((0xFF ^ gear_c[6]));
                                    gear_c2[5] = (byte)(gear_c2[6]);
                                    gear_c3[5] = (byte)((0xFF ^ gear_c3[6]));
                                    gear_c4[5] = (byte)(gear_c4[6]);



                                    /*~-1*/
                                }
                                /*~E:I95*/
                                /*~T*/

                                gear_c[7] = use_fixed_masks ? (byte)(gear_c[7] ^ gear_ergebnise1) : (byte)(gear_c[7] + gear_ergebnise1); // V4: XOR, V1-V3: ADD
                                gear_c2[7] = (byte)(gear_c2[7] ^ gear_ergebnise1_2);
                                gear_c3[7] = use_fixed_masks ? (byte)(gear_c3[7] ^ gear_ergebnise1_3) : (byte)(gear_c3[7] + gear_ergebnise1_3); // V4: XOR, V1-V3: ADD
                                gear_c4[7] = (byte)(gear_c4[7] ^ gear_ergebnise1_4);


                                /*~T*/



                                /*~I:96*/
                                if (takt >= 19)
                                /*~-1*/
                                {
                                    /*~T*/
                                    takt = 0;

                                    /*~-1*/
                                }
                                /*~E:I96*/
                                /*~-1*/
                            }
                            /*~E:I93*/
                            /*~E:A92*/
                            /*~T*/




                            takt++;
                            warte++;
                            bildwechsel++;

                            if (bildwechsel > 2000)
                            {
                                if (bildwechsel_merker == 0)
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_aus();
                                    }));
                                    bildwechsel_merker = 1;
                                }
                                else
                                {
                                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                                    {
                                        flame_ein();
                                    }));
                                    bildwechsel_merker = 0;
                                }
                                bildwechsel = 0;
                            }


                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                break;
                            }

                            fortschritt = (int)(((warte * 100) / dateiLaenge));

                            if ((fortschritt_merker < fortschritt) && (fortschritt < 101))
                            {
                                fortschritt_merker = fortschritt;
                                worker.ReportProgress((int)fortschritt);
                            }


                            /*_________________________________________________________________________*/


                            zeichenmenge++;

                            if ((dateiLaenge < block_laenge) || (zeichenmenge > ((dateiLaenge) - block_laenge)))
                            {
                                if (bald_ende == 0)
                                {
                                    if (richtung_info == 0)
                                    {
                                        for (int byteschreiber2 = 0; byteschreiber2 < zeichenanzahl; byteschreiber2++)
                                        {
                                            binWriter2.Write((byte)zeichenbuffer[byteschreiber2]);
                                        }
                                    }
                                    else
                                    {

                                        for (int byteschreiber2 = 0; byteschreiber2 < zeichenanzahl; byteschreiber2++)
                                        {
                                            binWriter2.Write((byte)(zeichenbuffer[byteschreiber2] ^ gearbuffer[byteschreiber2]));
                                        }


                                    }
                                    zeichenanzahl = 0;
                                    bald_ende = 1;
                                }
                                binWriter2.Write(((byte)(nurEinByte ^ gear_ergebnise1_out)));
                            }

                            else
                            {
                                if (richtung_info == 0)
                                {
                                    zeichenbuffer[zeichenanzahl] = ((byte)(nurEinByte ^ gear_ergebnise1_out));
                                }
                                else
                                {
                                    zeichenbuffer[zeichenanzahl] = ((byte)(nurEinByte));
                                    gearbuffer[zeichenanzahl] = (byte)gear_ergebnise1_out;
                                }
                                zeichenanzahl++;

                                if (zeichenanzahl == block_laenge)
                                {
                                    zeichenanzahl = 0;


                                    /*Blockcchiffere*/
                                    if ((passwort_wippe == 0) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1 ^ passwort_info_byte) % block_laenge);

                                        passwort_wippe = 1;
                                        wippe_merker = 1;
                                    }

                                    if ((passwort_wippe == 1) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1_2 ^ passwort_info_byte2) % block_laenge);

                                        passwort_wippe = 2;
                                        wippe_merker = 1;
                                    }

                                    if ((passwort_wippe == 2) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1_3 ^ passwort_info_byte3) % block_laenge);

                                        passwort_wippe = 3;
                                        wippe_merker = 1;
                                    }

                                    if ((passwort_wippe == 3) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1_4 ^ passwort_info_byte3_2) % block_laenge);

                                        passwort_wippe = 4;
                                        wippe_merker = 1;
                                    }

                                    if ((passwort_wippe == 4) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1 ^ passwort_info_byte3_3) % block_laenge);

                                        passwort_wippe = 5;
                                        wippe_merker = 1;
                                    }

                                    if ((passwort_wippe == 5) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1_2 ^ passwort_info_byte3_4) % block_laenge);

                                        passwort_wippe = 6;
                                        wippe_merker = 1;
                                    }

                                    if ((passwort_wippe == 6) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1_3 ^ passwort_info_byte3_5) % block_laenge);

                                        passwort_wippe = 7;
                                        wippe_merker = 1;
                                    }

                                    if ((passwort_wippe == 7) && (wippe_merker == 0))
                                    {

                                        passwort_info_byte4 = (byte)((gear_ergebnise1_4 ^ passwort_info_byte3_6) % block_laenge);

                                        passwort_wippe = 0;

                                    }

                                    if (passwort_info_byte4 == 0)
                                    {
                                        passwort_info_byte4 = (byte)((passwort_info_byte + gear_ergebnise1) % block_laenge);

                                    }

                                    if (passwort_info_byte4 == 0)
                                    {
                                        passwort_info_byte4 = 1;
                                    }

                                    wippe_merker = 0;
                                    //MessageBox.Show("   Ergebnis byte4=" + passwort_info_byte4);
                                    //MessageBox.Show("   Ergebnis block_laenge=" + block_laenge);







                                    if (erster_durchlauf == 0)
                                    {
                                        erster_durchlauf = 1;
                                        block_quersumme = (byte)(gear_ergebnisc1_3 ^ gear_ergebnisc2_4);
                                        block_summe = (byte)(gear_ergebnisd1_2 ^ gear_ergebnisb7_3);
                                    }


                                   // MessageBox.Show("   Laenge=" + block_laenge);
                                   // MessageBox.Show("   Schieber=" + ((gear_ergebnisc3_3 ^ gear_ergebnisc4_3 ^ block_quersumme) & 0xf));
                                    if (richtung_info == 0)
                                    {
                                        /*Verschlüsseln*/






                                        for (int schieber = 0; schieber < passwort_info_byte4; schieber++)
                                        {

                                            bytemerker = zeichenbuffer[block_laenge - 1];


                                            for (int schieber2 = block_laenge - 1; schieber2 >= 1; schieber2--)
                                            {

                                                zeichenbuffer[schieber2] = (byte)(zeichenbuffer[schieber2 - 1] ^ block_quersumme);
                                            }
                                            zeichenbuffer[0] = (byte)(bytemerker ^ block_quersumme);

                                        }

                                        for (int schieber = 0; schieber < ((gear_ergebnisc3_3 ^ gear_ergebnisc4_3 ^ block_quersumme) & 0xf); schieber++)
                                        {

                                            /*Beginn Bitverschiebung1*/
                                            schiebweite = (Byte)(gear_ergebnisb8_4 & 7);
                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {

                                                if (gear_ergebnisb8_4 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << (8 - schiebweite));
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2]);
                                            }

                                            schiebmerker = zeichenbuffer[0];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {
                                                zeichenbuffer[schieber2] = (byte)(bitschieber3[schieber2 + 1] | ((byte)(zeichenbuffer[schieber2] >> schiebweite)));
                                            }
                                            zeichenbuffer[block_laenge - 1] = (byte)(bitschieber3[0] | ((byte)(zeichenbuffer[block_laenge - 1] >> schiebweite)));
                                            /*Ende Bitverschiebung1*/
                                            /*Beginn Bitverschiebung2*/
                                            schiebweite = (Byte)(gear_ergebnisc2 & 7);
                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {

                                                if (gear_ergebnisc2 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << (8 - schiebweite));
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2]);
                                            }

                                            schiebmerker = zeichenbuffer[0];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {
                                                zeichenbuffer[schieber2] = (byte)(bitschieber3[schieber2 + 1] + ((byte)(zeichenbuffer[schieber2] >> schiebweite)));
                                            }
                                            zeichenbuffer[block_laenge - 1] = (byte)(bitschieber3[0] + ((byte)(zeichenbuffer[block_laenge - 1] >> schiebweite)));
                                            /*Ende Bitverschiebung2*/
                                            /*Beginn Bitverschiebung3*/
                                            schiebweite = (Byte)(gear_ergebnisd1 & 7);
                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {
                                                if (gear_ergebnisd1 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << (8 - schiebweite));
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2]);
                                            }

                                            schiebmerker = zeichenbuffer[0];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {
                                                zeichenbuffer[schieber2] = (byte)(bitschieber3[schieber2 + 1] + ((byte)(zeichenbuffer[schieber2] >> schiebweite)));
                                            }
                                            zeichenbuffer[block_laenge - 1] = (byte)(bitschieber3[0] + ((byte)(zeichenbuffer[block_laenge - 1] >> schiebweite)));
                                            /*Ende Bitverschiebung3*/
                                            /*Beginn Bitverschiebung4*/
                                            schiebweite = (Byte)(gear_ergebnisc2_2 & 7);
                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {
                                                if (gear_ergebnisc2_2 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << (8 - schiebweite));
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2]);


                                            }

                                            schiebmerker = zeichenbuffer[0];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {
                                                zeichenbuffer[schieber2] = (byte)(bitschieber3[schieber2 + 1] + ((byte)(zeichenbuffer[schieber2] >> schiebweite)));
                                            }
                                            zeichenbuffer[block_laenge - 1] = (byte)(bitschieber3[0] + ((byte)(zeichenbuffer[block_laenge - 1] >> schiebweite)));
                                            /*Ende Bitverschiebung4*/



                                        }

                                        for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                        {

                                            block_quersumme = (byte)(block_quersumme ^ zeichenbuffer[schieber2]);
                                            block_summe = (byte)(block_summe + zeichenbuffer[schieber2]);
                                        }
                                        block_quersumme = (byte)(block_quersumme ^ gear_ergebnisd2_3 ^ gear_ergebnisc3 ^ gear_ergebnisb3_4 ^ gear_ergebnisb4 ^ block_summe ^ gear_ergebnisc1_2 ^ gear_ergebnisb8_4 ^ gear_ergebnisa9_2 ^ gear_ergebnisa10 ^ gear_ergebnisa4_4 ^ gear_ergebnisb2 ^ gear_ergebnisc2_4 ^ gear_ergebnisa5_4);

                                    }

                                    else
                                    {
                                        if (erster_durchlauf_ent == 1)
                                        {

                                        }
                                        else
                                        {
                                            block_quersumme_merker = block_quersumme;
                                            block_summe_merker = block_summe;

                                            erster_durchlauf_ent = 1;

                                        }

                                        for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                        {

                                            block_quersumme_merker = (byte)(block_quersumme_merker ^ zeichenbuffer[schieber2]);
                                            block_summe_merker = (byte)(block_summe_merker + zeichenbuffer[schieber2]);


                                        }
                                        block_quersumme_merker = (byte)(block_quersumme_merker ^ gear_ergebnisd2_3 ^ gear_ergebnisc3 ^ gear_ergebnisb3_4 ^ gear_ergebnisb4 ^ block_summe_merker ^ gear_ergebnisc1_2 ^ gear_ergebnisb8_4 ^ gear_ergebnisa9_2 ^ gear_ergebnisa10 ^ gear_ergebnisa4_4 ^ gear_ergebnisb2 ^ gear_ergebnisc2_4 ^ gear_ergebnisa5_4);



                                        for (int schieber = 0; schieber < ((gear_ergebnisc3_3 ^ gear_ergebnisc4_3 ^ block_quersumme) & 0xf); schieber++)
                                        {
                                            /*Beginn Bitverschiebung1*/
                                            schiebweite = (Byte)(gear_ergebnisc2_2 & 7);


                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {

                                                if (gear_ergebnisc2_2 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << schiebweite);
                                                bitschieber[schieber2] = (ushort)(bitschieber[schieber2] & 0xFF00);
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2] >> 8);

                                            }


                                            schiebmerker = zeichenbuffer[block_laenge - 1];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {

                                                zeichenbuffer[schieber2 + 1] = (byte)(bitschieber3[schieber2] + ((byte)(zeichenbuffer[schieber2 + 1] << schiebweite)));
                                            }
                                            zeichenbuffer[0] = (byte)(bitschieber3[block_laenge - 1] + ((byte)(zeichenbuffer[0] << schiebweite)));
                                            /*Ende Bitverschiebung1*/
                                            /*Beginn Bitverschiebung2*/
                                            schiebweite = (Byte)(gear_ergebnisd1 & 7);


                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {
                                                if (gear_ergebnisd1 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << schiebweite);
                                                bitschieber[schieber2] = (ushort)(bitschieber[schieber2] & 0xFF00);
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2] >> 8);
                                            }


                                            schiebmerker = zeichenbuffer[block_laenge - 1];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {

                                                zeichenbuffer[schieber2 + 1] = (byte)(bitschieber3[schieber2] + ((byte)(zeichenbuffer[schieber2 + 1] << schiebweite)));
                                            }
                                            zeichenbuffer[0] = (byte)(bitschieber3[block_laenge - 1] + ((byte)(zeichenbuffer[0] << schiebweite)));
                                            /*Ende Bitverschiebung2*/
                                            /*Beginn Bitverschiebung3*/
                                            schiebweite = (Byte)(gear_ergebnisc2 & 7);


                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {
                                                if (gear_ergebnisc2 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << schiebweite);
                                                bitschieber[schieber2] = (ushort)(bitschieber[schieber2] & 0xFF00);
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2] >> 8);
                                            }


                                            schiebmerker = zeichenbuffer[block_laenge - 1];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {

                                                zeichenbuffer[schieber2 + 1] = (byte)(bitschieber3[schieber2] + ((byte)(zeichenbuffer[schieber2 + 1] << schiebweite)));
                                            }
                                            zeichenbuffer[0] = (byte)(bitschieber3[block_laenge - 1] + ((byte)(zeichenbuffer[0] << schiebweite)));
                                            /*Ende Bitverschiebung3*/
                                            /*Beginn Bitverschiebung4*/
                                            schiebweite = (Byte)(gear_ergebnisb8_4 & 7);


                                            for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
                                            {
                                                if (gear_ergebnisb8_4 < 128)
                                                { bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2]); }
                                                else
                                                {
                                                    bitschieber4[schieber2] = (byte)(zeichenbuffer[schieber2] ^ 0xFF);
                                                }
                                                bitschieber[schieber2] = (ushort)(bitschieber4[schieber2] << schiebweite);
                                                bitschieber[schieber2] = (ushort)(bitschieber[schieber2] & 0xFF00);
                                                bitschieber3[schieber2] = (byte)(bitschieber[schieber2] >> 8);
                                            }


                                            schiebmerker = zeichenbuffer[block_laenge - 1];
                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {

                                                zeichenbuffer[schieber2 + 1] = (byte)(bitschieber3[schieber2] | ((byte)(zeichenbuffer[schieber2 + 1] << schiebweite)));
                                            }
                                            zeichenbuffer[0] = (byte)(bitschieber3[block_laenge - 1] | ((byte)(zeichenbuffer[0] << schiebweite)));
                                            /*Ende Bitverschiebung4*/
                                        }


                                        for (int schieber = 0; schieber < passwort_info_byte4; schieber++)
                                        {

                                            bytemerker = zeichenbuffer[0];

                                            for (int schieber2 = 0; schieber2 < block_laenge - 1; schieber2++)
                                            {
                                                //MessageBox.Show("   Schieber=" + schieber2);
                                                zeichenbuffer[schieber2] = (byte)(zeichenbuffer[schieber2 + 1] ^ block_quersumme);
                                            }

                                            zeichenbuffer[block_laenge - 1] = (byte)(bytemerker ^ block_quersumme);



                                        }

                                        block_quersumme = block_quersumme_merker;
                                        block_summe = block_summe_merker;
                                    }


                                    /*Ende Blockchiffere*/


                                    if (richtung_info == 0)
                                    {
                                        for (int byteschreiber = 0; byteschreiber < block_laenge; byteschreiber++)
                                        {
                                            binWriter2.Write((byte)zeichenbuffer[byteschreiber]);
                                        }
                                    }
                                    else
                                    {
                                        for (int byteschreiber = 0; byteschreiber < block_laenge; byteschreiber++)
                                        {
                                            binWriter2.Write((byte)(zeichenbuffer[byteschreiber] ^ gearbuffer[byteschreiber]));
                                        }
                                    }


                                    block_modulo = (byte)(((gear_ergebnise1_2 ^ gear_ergebnise1_3) + passwort_info_byte) & 0xf);

                                    //MessageBox.Show("modulo=" + block_modulo);

                                    if (block_modulo < 5)
                                    {
                                        //block_modulo = 3;
                                        
                                        //Für die nächste Version
                                       
                                        block_modulo = (byte)(((gear_ergebnisb5_2 + gear_ergebnise1_3) ^ passwort_info_byte) & 0xf);
                                        if (block_modulo < 5)
                                        {
                                            
                                                block_modulo = (byte)(((gear_ergebnisa7_4 ^ gear_ergebnise1_2) ^ passwort_info_byte) & 0xf);
                                                if (block_modulo < 5)
                                                {
                                                    
                                                        block_modulo = (byte)(((gear_ergebnisb1_4 ^ gear_ergebnisa9_4) + passwort_info_byte) & 0xf);

                                                        if (block_modulo < 5)
                                                        {
                                                    block_modulo = 5;
                                                    }
                                                
                                                }

                                        }

                                        
                                      
                                        
                                       
                                    
                                    
                                    
                                    }


                                    if (passwort_wippe2 == 0)
                                    {
                                        block_laenge = (byte)((gear_ergebnise1_2 ^ passwort_info_byte) % block_modulo);
                                        passwort_wippe2 = 1;
                                        wippe_merker2 = 1;
                                        //MessageBox.Show("1" );
                                    }
                                    if ((wippe_merker2 == 0) && (passwort_wippe2 == 1))
                                    {
                                        block_laenge = (byte)((gear_ergebnise1_3 ^ passwort_info_byte2) % block_modulo);
                                        wippe_merker2 = 1;
                                        passwort_wippe2 = 2;
                                        //MessageBox.Show("2");
                                    }

                                    if ((wippe_merker2 == 0) && (passwort_wippe2 == 2))
                                    {
                                        block_laenge = (byte)((gear_ergebnise1_4 ^ passwort_info_byte3_2) % block_modulo);
                                        wippe_merker2 = 1;
                                        passwort_wippe2 = 3;
                                        //MessageBox.Show("3");
                                    }

                                    if ((wippe_merker2 == 0) && (passwort_wippe2 == 3))
                                    {
                                        block_laenge = (byte)((gear_ergebnise1 ^ passwort_info_byte3_3) % block_modulo);
                                        //MessageBox.Show("Laenge: "+block_laenge);
                                        passwort_wippe2 = 0;
                                    }

                                    wippe_merker2 = 0;

                                    //MessageBox.Show("Block_Laenge: " + block_laenge);

                                    if (block_laenge < 3)
                                    {
                                        //block_laenge = 2;
                                        //Für die nächste Version
                                       
                                        block_laenge = (byte)((gear_ergebnisb6_2 ^ passwort_info_byte3_3 + gear_ergebnisa8_2) % block_modulo);
                                        if (block_laenge < 3)
                                        {
                                            block_laenge = (byte)((gear_ergebnisa9_3 + passwort_info_byte3_3 ^ gear_ergebnisb1_3) % block_modulo);
                                            if (block_laenge < 3)
                                            {
                                                block_laenge = 3;
                                            }  
                                        }
                                       
                                    }
                                    //MessageBox.Show("Block_Laenge: " + block_laenge);
                                }

                            }
                            //binWriter2.Write(((byte)(nurEinByte ^ gear_ergebnise1)));













                        }

                        fs.Close();
                        binWriter2.Close();

                        if (richtung_info == 0)
                        {
                            ShowFg("Encryption is completed successfully!");
                        }
                        else
                        {
                            ShowFg((falsche_datei == 0)
                                ? "Decoding is completed successfully!"
                                : "Decoding canceled!");
                        }


                        //MessageBox.Show("Ver-/Entschlüsselung erfolgreich beendet!");


                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                        {
                            flame_aus();
                        }));

                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                        {
                            label5.Foreground = Brushes.Black;
                            SetBurningLogo(false);
                            // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                            image6.Visibility = Visibility.Hidden;
                            image1.Visibility = Visibility.Visible;
                            /*
                            button2.Visibility = Visibility.Visible;
                            button5.Visibility = Visibility.Visible;
                            button4.Visibility = Visibility.Visible;
                            textBox1.Visibility = Visibility.Visible;
                            button3.Visibility = Visibility.Visible;
                            textBox2.Visibility = Visibility.Visible;
                            button6.Visibility = Visibility.Visible;
                            label3.Visibility = Visibility.Visible;
                            image5.Visibility = Visibility.Visible;
                            label1.Visibility = Visibility.Visible;
                            progressBar2.Visibility = Visibility.Visible;
                            label2.Visibility = Visibility.Visible;
                            textBox3.Visibility = Visibility.Visible;
                            label4.Visibility = Visibility.Visible;
                            textBox4.Visibility = Visibility.Visible;
                            radioButton1.Visibility = Visibility.Visible;
                            radioButton2.Visibility = Visibility.Visible;
                            textBox5.Visibility = Visibility.Visible;*/
                        }));



                        prozess_laueft = false;
                        worker.ReportProgress((int)0);




                        /*____________________________________________________________________________*/





                    }
                    else
                    {

                        if (dateil1 == 0)
                        {
                            ShowFg("Error: The source file is not selected!");
                        }

                        if (dateil2 == 0)
                        {
                            ShowFg("Error: The target file is not selected!");
                        }

                        if (passwortgroesse > 1024)
                        {
                            ShowFg("Error: The Key is too long  (maximum 1024 characters)!");
                        }

                        if (passwortgroesse <= 5)
                        {
                            ShowFg("Error: The Key is too short (minimum 6 characters)!");
                        }

                        if (!(passwort1.Equals(passwort2)) && (!radioButton1_global))
                        {
                            ShowFg("Error: Keys are not equal!");
                        }

                        if (((dateigroesse1.Equals(dateigroesse2))))
                        {
                            ShowFg("Error: The source file may not be the target file!");
                        }


                        prozess_laueft = false;
                        //image2.Visibility = Visibility.Hidden;
                        //image3.Visibility = Visibility.Visible;

                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                        {
                            flame_aus();
                        }));
                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                        {
                            label5.Foreground = Brushes.Black;
                            SetBurningLogo(false);
                            // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                            image6.Visibility = Visibility.Hidden;
                            image1.Visibility = Visibility.Visible;

                            /*
                            button2.Visibility = Visibility.Visible;
                            button5.Visibility = Visibility.Visible;
                            button4.Visibility = Visibility.Visible;
                            textBox1.Visibility = Visibility.Visible;
                            button3.Visibility = Visibility.Visible;
                            textBox2.Visibility = Visibility.Visible;
                            button6.Visibility = Visibility.Visible;
                            label3.Visibility = Visibility.Visible;
                            image5.Visibility = Visibility.Visible;
                            label1.Visibility = Visibility.Visible;
                            progressBar2.Visibility = Visibility.Visible;
                            label2.Visibility = Visibility.Visible;
                            textBox3.Visibility = Visibility.Visible;
                            label4.Visibility = Visibility.Visible;
                            textBox4.Visibility = Visibility.Visible;
                            radioButton1.Visibility = Visibility.Visible;
                            radioButton2.Visibility = Visibility.Visible;
                            textBox5.Visibility = Visibility.Visible;*/
                        }));

                    }






                    /*_______________TURBINEENDE___________________________*/

                }

                else
                {
                    /*verschlüssele mit AES*/



                }

            }//Ende von Try

            catch
            {


                ShowFg("Start not possible! Check file properties and permissions!");
                prozess_laueft = false;


                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    flame_aus();
                }));
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    label5.Foreground = Brushes.Black;
                    SetBurningLogo(false);
                    // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                    image6.Visibility = Visibility.Hidden;
                    image1.Visibility = Visibility.Visible;
                    /*
                    button2.Visibility = Visibility.Visible;
                    button5.Visibility = Visibility.Visible;
                    button4.Visibility = Visibility.Visible;
                    textBox1.Visibility = Visibility.Visible;
                    button3.Visibility = Visibility.Visible;
                    textBox2.Visibility = Visibility.Visible;
                    button6.Visibility = Visibility.Visible;
                    label3.Visibility = Visibility.Visible;
                    image5.Visibility = Visibility.Visible;
                    label1.Visibility = Visibility.Visible;
                    progressBar2.Visibility = Visibility.Visible;
                    label2.Visibility = Visibility.Visible;
                    textBox3.Visibility = Visibility.Visible;
                    label4.Visibility = Visibility.Visible;
                    textBox4.Visibility = Visibility.Visible;
                    radioButton1.Visibility = Visibility.Visible;
                    radioButton2.Visibility = Visibility.Visible;
                    textBox5.Visibility = Visibility.Visible;*/


                }));

            }


        }

        // This event handler deals with the results of the
        // background operation.
        // ===== HELPER 2026-05-20 =====
        // Zentrale Anzeige von MessageBoxen mit:
        //   1. Marshalling auf den UI-Thread (falls aus Background-Thread aufgerufen)
        //   2. Window-in-Vordergrund holen (Activate + Topmost-Trick + Focus)
        //   3. MessageBox mit Owner-Parameter (this) - bleibt modal vor dem Fenster
        // Verwendung: anstelle von MessageBox.Show(...) -> ShowFg(...)
        // Hintergrund: Nach .NET 3.5 -> 4.8 Migration kamen MessageBoxes aus
        // BackgroundWorker-Threads nicht mehr zuverlaessig in den Vordergrund.
        // ----------------------------------------------------------------
        // SetBurningLogo - schaltet zwischen normalem und brennendem Logo um
        // ----------------------------------------------------------------
        // 2026-05-20: Ersetzt den frueheren label5.Foreground=Red Trick.
        // Wird parallel zu den vorhandenen label5.Foreground-Aufrufen aufgerufen,
        // damit Fallback erhalten bleibt (label5 ist im XAML nur Hidden, nicht entfernt).
        // Per Dispatcher.Invoke aufrufen, falls aus BackgroundWorker-Thread aufgerufen.
        private void SetBurningLogo(bool burning)
        {
            Action a = new Action(delegate()
            {
                if (image_logo_normal != null && image_logo_burning != null)
                {
                    image_logo_normal.Visibility  = burning ? System.Windows.Visibility.Hidden  : System.Windows.Visibility.Visible;
                    image_logo_burning.Visibility = burning ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                }
            });
            if (this.Dispatcher.CheckAccess()) a();
            else this.Dispatcher.Invoke(a);
        }

        private void ShowFg(string text)
        {
            Action a = new Action(delegate()
            {
                if (this.WindowState == System.Windows.WindowState.Minimized)
                    this.WindowState = System.Windows.WindowState.Normal;
                this.Activate();
                this.Topmost = true;
                this.Topmost = false;
                this.Focus();
                MessageBox.Show(this, text);
            });
            if (this.Dispatcher.CheckAccess()) a();
            else this.Dispatcher.Invoke(a);
        }

        private void ShowFg(string text, string caption)
        {
            Action a = new Action(delegate()
            {
                if (this.WindowState == System.Windows.WindowState.Minimized)
                    this.WindowState = System.Windows.WindowState.Normal;
                this.Activate();
                this.Topmost = true;
                this.Topmost = false;
                this.Focus();
                MessageBox.Show(this, text, caption);
            });
            if (this.Dispatcher.CheckAccess()) a();
            else this.Dispatcher.Invoke(a);
        }
        // ===== Ende HELPER =====

        private void backgroundWorker1_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                ShowFg(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                //resultLabel.Text = "Canceled";
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                //resultLabel.Text = e.Result.ToString();
            }


        }


        private void backgroundWorker2_RunWorkerCompleted(
           object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                ShowFg(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                // Next, handle the case where the user canceled 
                // the operation.
                // Note that due to a race condition in 
                // the DoWork event handler, the Cancelled
                // flag may not have been set, even though
                // CancelAsync was called.
                //resultLabel.Text = "Canceled";
            }
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                //resultLabel.Text = e.Result.ToString();
            }


        }




        // This event handler updates the progress bar.
        private void backgroundWorker1_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;

            //this.progressBar1.Value = 50;
        }




        private void backgroundWorker2_ProgressChanged(object sender,
         ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;

            //this.progressBar1.Value = 50;
        }





        private byte[] StringToByteArray(string str)
        {
            System.Text.UTF8Encoding enc2 = new System.Text.UTF8Encoding();
            // System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc2.GetBytes(str);


        }

        private string ByteToString2(byte[] ByteArray)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            foreach (byte b in ByteArray)
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            return sb.ToString();
        }







        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string zieldatei;




            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "txt";
            saveDialog.AddExtension = true;
            saveDialog.FileName = "filename.abc";
            saveDialog.InitialDirectory = @"C:\Users\<Ihr Name>\Documents\";
            saveDialog.OverwritePrompt = true;
            saveDialog.Title = "Turbine";
            saveDialog.ValidateNames = true;

            if (saveDialog.ShowDialog().Value)
            {
                using (StreamWriter writer = new StreamWriter(saveDialog.FileName))
                {
                    textBox2.Text = saveDialog.FileName;
                    zieldatei = saveDialog.FileName;


                }
            }


        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

            string quelldatei;
            // Standarddialog zum Öffnen anzeigen
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.Title = "Open a file";

            openFileDialog1.ShowDialog();


            textBox1.Text = openFileDialog1.FileName;
            quelldatei = openFileDialog1.FileName;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            string ziel;





            if (prozess_laueft == false)
            {
                label5.Foreground = Brushes.Red;
                SetBurningLogo(true);
                // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                image6.Visibility = Visibility.Visible;
                image1.Visibility = Visibility.Hidden;
                /*
                button2.Visibility = Visibility.Hidden;
                button5.Visibility= Visibility.Hidden;
                button4.Visibility=Visibility.Hidden;
                textBox1.Visibility = Visibility.Hidden;
                button3.Visibility = Visibility.Hidden;
                textBox2.Visibility = Visibility.Hidden;
                button6.Visibility = Visibility.Hidden;
                label3.Visibility = Visibility.Hidden;
                image5.Visibility = Visibility.Hidden;
                label1.Visibility= Visibility.Hidden;
                progressBar2.Visibility = Visibility.Hidden;
                label2.Visibility = Visibility.Hidden;
                textBox3.Visibility = Visibility.Hidden;
                label4.Visibility = Visibility.Hidden;
                textBox4.Visibility = Visibility.Hidden;
                radioButton1.Visibility = Visibility.Hidden;
                radioButton2.Visibility = Visibility.Hidden;
                textBox5.Visibility = Visibility.Hidden;*/

                /* Alt: Auswahl Ent/Verschlüsselung über Buttons
                if ((bool)radioButton4.IsChecked)
                {
                    richtung_info = 0; //Verschlüsseln
                }

                if ((bool)radioButton3.IsChecked)
                {
                    richtung_info = 1; // Entschlüsseln
                }*/


                prozess_laueft = true;
                passwortgroesse = 0;

                if (passwort_anzeige == 1)
                {

                    passwort1 = textBox5.Text;
                }
                else
                {

                    passwort1 = textBox3.Password;
                }


                //passwort1 = textBox3.Password;

                passwort2 = textBox4.Password;

                dateigroesse1 = textBox1.Text;
                dateigroesse2 = textBox2.Text;

                dateil1 = dateigroesse1.Length;
                dateil2 = dateigroesse2.Length;




                if (schluesseldatei_geladen == 0)
                {
                    if (passwort_anzeige == 1)
                    {

                        name_der_datei6 = StringToByteArray(textBox5.Text);
                    }
                    else
                    {

                        name_der_datei6 = StringToByteArray(textBox3.Password);
                    }



                    passwortgroesse = passwort1.Length;
                    gen_passwort = passwort1.Length;
                }

                else
                {
                    name_der_datei6 = name_der_datei6X;
                    passwort1 = "xxxyyy";
                    passwort2 = "xxxyyy";
                    passwortgroesse = 1023;
                    gen_passwort = 1023;

                }

                radioButton1_global = ((bool)radioButton1.IsChecked);
                algo = true;
                fortschritt = 0;
                fortschritt_merker = 0;


                /*----------------------------------------------*/

                /*
                try
                {
                    SaveFileDialog saveDialogX = new SaveFileDialog();
                    saveDialogX.DefaultExt = "txt";
                    saveDialogX.AddExtension = true;
                    saveDialogX.FileName = "DateinameX.abc";
                    saveDialogX.InitialDirectory = @"C:\Dokumente und Einstellungen\Notebook\Desktop\";
                    saveDialogX.OverwritePrompt = true;
                    saveDialogX.Title = "Turbine";
                    saveDialogX.ValidateNames = true;

                    if (saveDialogX.ShowDialog().Value)
                    {
                        using (StreamWriter writer = new StreamWriter(saveDialogX.FileName))
                        {
                            //textBox2.Text = saveDialog.FileName;
                            ziel = saveDialogX.FileName;


                        }
                    }


                    ziel = saveDialogX.FileName;


                    FileStream keyfile2 = new FileStream(@ziel, FileMode.OpenOrCreate, FileAccess.Write);
                    BinaryWriter keyreader2 = new BinaryWriter(keyfile2);

                    for (long i = 0; i < passwortgroesse; i++)
                    {
                        keyreader2.Write(name_der_datei6[i]);
                    }
                    keyfile2.Close();
                   
                }
                catch {

                    MessageBox.Show("X");
                }
                */



                /*----------------------------------------------*/




                backgroundWorker1.RunWorkerAsync();
            }


        }

        private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {


        }

        private void image1_ImageFailed_1(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string quelldatei;
            // Standarddialog zum Öffnen anzeigen
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.Title = "Open a file";


            openFileDialog1.ShowDialog();

            quelldatei = "";



            textBox1.Text = openFileDialog1.FileName;
            quelldatei = openFileDialog1.FileName;
            textBox1.Visibility = Visibility.Visible;
            button2.Background = Brushes.Green;


            
            
            //datei_endung_info1

            if (quelldatei != "")
            {
                // Debug-Anzeige des Pfads entfernt (war Entwicklungs-Hilfe)
                datei_endung1 = openFileDialog1.SafeFileName;
                datei_endung_info1 = datei_endung1.IndexOf('.');
                datei_endung_info2 = (datei_endung1.Length) - (datei_endung_info1+1);


                if (datei_endung_info2 == 3)
                {
                    //MessageBox.Show("datei_endung_info2==3");
                    datei_endung2 = datei_endung1.Substring(datei_endung_info1 + 1, 3);
                    datei_endung_info3 = 3;
                }

                if (datei_endung_info2 == 4)
                {
                    //MessageBox.Show("datei_endung_info2==4");
                    datei_endung2 = datei_endung1.Substring(datei_endung_info1 + 1, 4);
                    datei_endung_info3 = 4;
                }



                
                   
                   
               //datei_endung2 = datei_endung1.Substring(datei_endung_info1 + 1, 3);
                

                


                // Einfügen Endung Abfrage:

                // Datei öffnen und den Inhalt byteweise auslesen 
                FileInfo fi = new FileInfo(@quelldatei);

                //FileStream fs = new FileStream(@dateigroesse1, FileMode.Open);
                FileStream fs = new FileStream(@quelldatei, FileMode.Open, FileAccess.Read);







                for (long s = 0; s < 43; s++)/*Lese die ersten 43 BMP Bytes der verschlüsselten Datei*/
                {
                    dummy_byte1 = (byte)fs.ReadByte();
                }

                dummy_byte1 = (byte)fs.ReadByte(); //Lese TURBINE um zu prüfen, ob mit Turbine verschlüsselt wurde
                dummy_byte2 = (byte)fs.ReadByte();
                dummy_byte3 = (byte)fs.ReadByte();
                dummy_byte4 = (byte)fs.ReadByte();
                dummy_byte5 = (byte)fs.ReadByte();
                dummy_byte6 = (byte)fs.ReadByte();
                dummy_byte7 = (byte)fs.ReadByte();

                dummy_byte8 = (byte)fs.ReadByte(); //enthält den ehemaligen suffix
                dummy_byte9 = (byte)fs.ReadByte();
                dummy_byte10 = (byte)fs.ReadByte();
                dummy_byte11 = (byte)fs.ReadByte();

                Turbine_Name[0] = dummy_byte1;
                Turbine_Name[1] = dummy_byte2;
                Turbine_Name[2] = dummy_byte3;
                Turbine_Name[3] = dummy_byte4;
                Turbine_Name[4] = dummy_byte5;
                Turbine_Name[5] = dummy_byte6;
                Turbine_Name[6] = dummy_byte7;

                Turbine_Typ_Endung[0] = dummy_byte8;
                Turbine_Typ_Endung[1] = dummy_byte9;
                Turbine_Typ_Endung[2] = dummy_byte10;
                Turbine_Typ_Endung[3] = dummy_byte11;


                Turbine_Header = ByteArrayToString(Turbine_Name);
                Turbine_Typ = ByteArrayToString(Turbine_Typ_Endung);





                if ((dummy_byte1 == 'T') && (dummy_byte2 == 'U') && (dummy_byte3 == 'R') && (dummy_byte4 == 'B') && (dummy_byte5 == 'I') &&
                    (dummy_byte6 == 'N') && (dummy_byte7 == 'E'))//Ist TURBINE im BMP Header enthalten?
                {
                    ShowFg(Turbine_Typ, "Turbine encrypted file identified! Original File Type is:"); //File ist mit Turbine verschlüsselt worden. Setze entschlüsseln.
                    //radioButton3.IsChecked = true;
                    //radioButton3.Foreground = new SolidColorBrush(Colors.Black);
                    //radioButton3.FontWeight = FontWeights.Heavy;
                    

                    //radioButton4.IsChecked = false;
                    //radioButton4.Foreground = new SolidColorBrush(Colors.Gray);
                    //radioButton4.FontWeight = FontWeights.Normal;

                    richtung_info = 1;
                    label6.Content = "Decryption Mode";
                    label6.FontWeight = FontWeights.Heavy;
                    label6.Foreground = new SolidColorBrush(Colors.Red);

                }


                else
                {
                    //radioButton3.IsChecked = false; //Keine Turbine Datei. Setze verschlüsseln
                    //radioButton3.Foreground = new SolidColorBrush(Colors.Gray);
                    //radioButton3.FontWeight = FontWeights.Normal;

                    //radioButton4.IsChecked = true;
                    //radioButton4.Foreground = new SolidColorBrush(Colors.Black);
                    //radioButton4.FontWeight = FontWeights.Heavy;

                    richtung_info = 0;
                    label6.Content = "Encryption Mode";
                    label6.FontWeight = FontWeights.Heavy;
                    label6.Foreground = new SolidColorBrush(Colors.Red);
                }


                fs.Close();

            }


        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            string zieldatei;


            



            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();

                if (richtung_info == 0) //Wird verschlüsselt?
                {


                    saveDialog.FileName = "*.tur";
                    saveDialog.AddExtension = true;
                    saveDialog.DefaultExt = "tur";


                }
                else
                {
                    saveDialog.FileName = "*."+Turbine_Typ;
                    saveDialog.DefaultExt = Turbine_Typ;
                }
                saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveDialog.OverwritePrompt = true;
                saveDialog.Title = "Turbine";
                saveDialog.ValidateNames = true;

                if (saveDialog.ShowDialog().Value)
                {
                    textBox2.Text = saveDialog.FileName;
                    zieldatei = saveDialog.FileName;
                }
                textBox2.Visibility = Visibility.Visible;
                button3.Background = Brushes.Green;

            }

            catch
            {
                ShowFg("Access not possible!");
            }

        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            ShowFg("Turbine V5.2 is a free encryption program.\n\n" +
                "Cipher: 1280-bit internal state across 4 parallel gear groups, stop-go-clocked\n" +
                "shift register design (related family: Trivium).\n\n" +
                "Key derivation:\n" +
                "  V2 - Password mode (PBKDF2-SHA512, 1,200,000 iterations) - format byte 0x01\n" +
                "  V3 - Key-file mode with SHA-512 whitening - format byte 0x03\n" +
                "  Legacy modes (0x00, 0x02) remain readable for backward compatibility.\n\n" +
                "Password length: 6 to 1024 characters.\n" +
                "Key-file: any file >= 7 KB (photos, archives, or previously encrypted .tur files).\n\n" +
                "Open source, MIT licensed, no backdoor.\n" +
                "Source code: https://github.com/ReinhardJesolowitz24/Turbine\n\n" +
                "This program is NOT to be used for any illegal activities, particularly\n" +
                "pornographic, violent or discriminatory purposes.\n\n" +
                "Use at your own risk! No liability for consequential damage or data loss.");
            //MessageBox.Show(datei_endung1);
            //MessageBox.Show(datei_endung2);
            //String[] substrings = value.Split(delimiter)

            

        
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, TextChangedEventArgs e)
        {
            int byte0 = 0;
            int byte1 = 0;
            int byte2 = 0;
            int byte3 = 0;
            int byte4 = 0;
            int byte5 = 0;

            int passwort_groesster_wert = 0;

            int passwort_kleinster_wert = 255;

            int passwortgroesse_lokal = 0;

            string passwort_lokal;

            schluesseldatei_geladen = 0;
            button6.Background = Brushes.White;
            progressBar2.Visibility = Visibility.Visible;
            label2.Visibility = Visibility.Hidden;
            label1.Visibility = Visibility.Visible;
            //textBox3.Background = Brushes.White;

            //textBox4.Visibility = Visibility.Visible;
            image5.Visibility = Visibility.Hidden;

            /* if ((bool)radioButton1.IsChecked)
             {
                 textBox5.Visibility = Visibility.Visible;
             }*/

            //textBox5.Visibility = Visibility.Visible;
            //label4.Visibility = Visibility.Visible;
            radioButton1.Visibility = Visibility.Visible;
            radioButton2.Visibility = Visibility.Visible;

            /*
                        if ((bool)radioButton1.IsChecked)
                        {
                            textBox5.Text = textBox3.Password;
                        }*/

            /*dateigroesse1 = textBox1.Text;
                dateigroesse2 = textBox2.Text;

                dateil1 = dateigroesse1.Length;
                dateil2 = dateigroesse2.Length;



                name_der_datei6 = StringToByteArray(textBox3.Password);
                passwortgroesse = passwort1.Length;
                gen_passwort = passwort1.Length;*/

            if (passwort_anzeige == 1)
            {
                name_der_datei6 = StringToByteArray(textBox5.Text);
                passwort_lokal = textBox5.Text;
            }
            else
            {
                name_der_datei6 = StringToByteArray(textBox3.Password);
                passwort_lokal = textBox3.Password;
            }



            passwortgroesse_lokal = passwort_lokal.Length;
            /*gen_passwort = passwort1.Length;*/




            if (passwortgroesse_lokal > 5)
            {
                progressBar2.Value = 20;
                progressBar2.Foreground = Brushes.Red;


                byte0 = (int)name_der_datei6[0];
                byte1 = (int)name_der_datei6[1];
                byte2 = (int)name_der_datei6[2];
                byte3 = (int)name_der_datei6[3];
                byte4 = (int)name_der_datei6[4];
                byte5 = (int)name_der_datei6[5];

                for (int laufe = 0; laufe < passwortgroesse_lokal; laufe++)
                {
                    if (name_der_datei6[laufe] > passwort_groesster_wert)
                    {

                        passwort_groesster_wert = name_der_datei6[laufe];
                    }
                }


                for (int laufe = 0; laufe < passwortgroesse_lokal; laufe++)
                {
                    if (name_der_datei6[laufe] < passwort_kleinster_wert)
                    {
                        passwort_kleinster_wert = name_der_datei6[laufe];
                    }


                }

                if (((passwort_groesster_wert - passwort_kleinster_wert) > 10) && ((passwortgroesse_lokal > 6)))
                {
                    progressBar2.Value = 40;
                    progressBar2.Foreground = Brushes.Orange;
                    if (((passwort_groesster_wert - passwort_kleinster_wert) > 40) && (passwortgroesse_lokal > 8) || ((passwort_groesster_wert - passwort_kleinster_wert) > 30) && (passwortgroesse_lokal > 9) || ((passwort_groesster_wert - passwort_kleinster_wert) > 20) && (passwortgroesse_lokal > 10) || ((passwort_groesster_wert - passwort_kleinster_wert) > 10) && (passwortgroesse_lokal > 12))
                    {
                        progressBar2.Value = 60;
                        progressBar2.Foreground = Brushes.Yellow;

                        //MessageBox.Show("kw= "+passwort_kleinster_wert);
                        //MessageBox.Show("gw= "+passwort_groesster_wert);
                        if ((((passwort_groesster_wert - passwort_kleinster_wert) > 60) && (passwortgroesse_lokal > 11)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 40) && (passwortgroesse_lokal > 13)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 30) && (passwortgroesse_lokal > 15)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 20) && (passwortgroesse_lokal > 20)))
                        {
                            progressBar2.Value = 80;
                            progressBar2.Foreground = Brushes.MediumSeaGreen;

                            if (((((passwort_groesster_wert - passwort_kleinster_wert) > 65) && (passwortgroesse_lokal > 12)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 60) && (passwortgroesse_lokal > 20)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 40) && (passwortgroesse_lokal > 23)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 20) && (passwortgroesse_lokal > 33))) && (passwortgroesse_lokal > 12))
                            {
                                progressBar2.Value = 100;
                                progressBar2.Foreground = Brushes.Green;
                                image5.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                progressBar2.Value = 80;
                                image5.Visibility = Visibility.Hidden;
                            }

                        }

                        else
                        {
                            progressBar2.Value = 60;
                            image5.Visibility = Visibility.Hidden;

                        }



                    }
                    else
                    {
                        progressBar2.Value = 40;
                        image5.Visibility = Visibility.Hidden;
                    }


                }

                else
                {
                    progressBar2.Value = 20;
                    image5.Visibility = Visibility.Hidden;
                }

            }
            else
            {

                if ((passwortgroesse_lokal > 0))
                {
                    progressBar2.Value = 10;
                    progressBar2.Foreground = Brushes.DarkRed;
                    image5.Visibility = Visibility.Hidden;
                }
                else
                {
                    progressBar2.Value = 0;
                    image5.Visibility = Visibility.Hidden;
                }

            }

            //progressBar2.Value++;
        }

        private void textBox3_PasswordChanged(object sender, RoutedEventArgs e)
        {
            int byte0 = 0;
            int byte1 = 0;
            int byte2 = 0;
            int byte3 = 0;
            int byte4 = 0;
            int byte5 = 0;

            int passwort_groesster_wert = 0;

            int passwort_kleinster_wert = 255;

            int passwortgroesse_lokal = 0;

            string passwort_lokal;

            schluesseldatei_geladen = 0;
            button6.Background = Brushes.White;
            progressBar2.Visibility = Visibility.Visible;
            label2.Visibility = Visibility.Hidden;
            label1.Visibility = Visibility.Visible;
            //textBox3.Background = Brushes.White;

            textBox4.Visibility = Visibility.Visible;
            image5.Visibility = Visibility.Hidden;

            if ((bool)radioButton1.IsChecked)
            {
                textBox5.Visibility = Visibility.Visible;
            }

            //textBox5.Visibility = Visibility.Visible;
            label4.Visibility = Visibility.Visible;
            radioButton1.Visibility = Visibility.Visible;
            radioButton2.Visibility = Visibility.Visible;


            if ((bool)radioButton1.IsChecked)
            {
                textBox5.Text = textBox3.Password;
            }

            /*dateigroesse1 = textBox1.Text;
                dateigroesse2 = textBox2.Text;

                dateil1 = dateigroesse1.Length;
                dateil2 = dateigroesse2.Length;



                name_der_datei6 = StringToByteArray(textBox3.Password);
                passwortgroesse = passwort1.Length;
                gen_passwort = passwort1.Length;*/

            if (passwort_anzeige == 1)
            {
                name_der_datei6 = StringToByteArray(textBox5.Text);
                passwort_lokal = textBox5.Text;
            }
            else
            {
                name_der_datei6 = StringToByteArray(textBox3.Password);
                passwort_lokal = textBox3.Password;
            }



            passwortgroesse_lokal = passwort_lokal.Length;
            /*gen_passwort = passwort1.Length;*/




            if (passwortgroesse_lokal > 5)
            {
                progressBar2.Value = 20;
                progressBar2.Foreground = Brushes.Red;


                byte0 = (int)name_der_datei6[0];
                byte1 = (int)name_der_datei6[1];
                byte2 = (int)name_der_datei6[2];
                byte3 = (int)name_der_datei6[3];
                byte4 = (int)name_der_datei6[4];
                byte5 = (int)name_der_datei6[5];

                for (int laufe = 0; laufe < passwortgroesse_lokal; laufe++)
                {
                    if (name_der_datei6[laufe] > passwort_groesster_wert)
                    {

                        passwort_groesster_wert = name_der_datei6[laufe];
                    }
                }


                for (int laufe = 0; laufe < passwortgroesse_lokal; laufe++)
                {
                    if (name_der_datei6[laufe] < passwort_kleinster_wert)
                    {
                        passwort_kleinster_wert = name_der_datei6[laufe];
                    }


                }

                if (((passwort_groesster_wert - passwort_kleinster_wert) > 10) && ((passwortgroesse_lokal > 6)))
                {
                    progressBar2.Value = 40;
                    progressBar2.Foreground = Brushes.Orange;
                    if (((passwort_groesster_wert - passwort_kleinster_wert) > 40) && (passwortgroesse_lokal > 8) || ((passwort_groesster_wert - passwort_kleinster_wert) > 30) && (passwortgroesse_lokal > 9) || ((passwort_groesster_wert - passwort_kleinster_wert) > 20) && (passwortgroesse_lokal > 10) || ((passwort_groesster_wert - passwort_kleinster_wert) > 10) && (passwortgroesse_lokal > 12))
                    {
                        progressBar2.Value = 60;
                        progressBar2.Foreground = Brushes.Yellow;

                        //MessageBox.Show("kw= "+passwort_kleinster_wert);
                        //MessageBox.Show("gw= "+passwort_groesster_wert);
                        if ((((passwort_groesster_wert - passwort_kleinster_wert) > 60) && (passwortgroesse_lokal > 11)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 40) && (passwortgroesse_lokal > 13)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 30) && (passwortgroesse_lokal > 15)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 20) && (passwortgroesse_lokal > 20)))
                        {
                            progressBar2.Value = 80;
                            progressBar2.Foreground = Brushes.MediumSeaGreen;

                            if (((((passwort_groesster_wert - passwort_kleinster_wert) > 65) && (passwortgroesse_lokal > 12)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 60) && (passwortgroesse_lokal > 20)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 40) && (passwortgroesse_lokal > 23)) || (((passwort_groesster_wert - passwort_kleinster_wert) > 20) && (passwortgroesse_lokal > 33))) && (passwortgroesse_lokal > 12))
                            {
                                progressBar2.Value = 100;
                                progressBar2.Foreground = Brushes.Green;
                                image5.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                progressBar2.Value = 80;
                                image5.Visibility = Visibility.Hidden;
                            }

                        }

                        else
                        {
                            progressBar2.Value = 60;
                            image5.Visibility = Visibility.Hidden;

                        }



                    }
                    else
                    {
                        progressBar2.Value = 40;
                        image5.Visibility = Visibility.Hidden;
                    }


                }

                else
                {
                    progressBar2.Value = 20;
                    image5.Visibility = Visibility.Hidden;
                }

            }
            else
            {

                if ((passwortgroesse_lokal > 0))
                {
                    progressBar2.Value = 10;
                    progressBar2.Foreground = Brushes.DarkRed;
                    image5.Visibility = Visibility.Hidden;
                }
                else
                {
                    progressBar2.Value = 0;
                    image5.Visibility = Visibility.Hidden;
                }

            }

            //progressBar2.Value++;

        }

        private void radioButton1_Checked_1(object sender, RoutedEventArgs e)
        {
            if ((bool)radioButton1.IsChecked)
            {
                textBox5.Text = textBox3.Password;
                textBox5.Visibility = Visibility.Visible;
                textBox3.Visibility = Visibility.Hidden;
                textBox4.Visibility = Visibility.Hidden;
                label4.Visibility = Visibility.Hidden;
                passwort_anzeige = 1;
            }


        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            textBox3.Password = textBox5.Text;
            //textBox4.Password = "";
            textBox5.Visibility = Visibility.Hidden;
            textBox3.Visibility = Visibility.Visible;
            textBox4.Visibility = Visibility.Visible;
            label4.Visibility = Visibility.Visible;
            passwort_anzeige = 0;
        }

        private void progressBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {


        }



        private void button6_Click(object sender, RoutedEventArgs e)
        {
            schluesseldatei_geladen = 0;
            button6.Background = Brushes.White;
            progressBar2.Visibility = Visibility.Visible;
            label2.Visibility = Visibility.Hidden;
            label1.Visibility = Visibility.Visible;
            textBox3.Background = Brushes.White;
            textBox4.Visibility = Visibility.Visible;
            image5.Visibility = Visibility.Hidden;

            if ((bool)radioButton1.IsChecked)
            {
                textBox5.Visibility = Visibility.Visible;
            }
            label4.Visibility = Visibility.Visible;
            radioButton1.Visibility = Visibility.Visible;
            radioButton2.Visibility = Visibility.Visible;


            byte x;
            string schluesseldatei;
            byte[] schluessel = new byte[1024];
            byte[] y = new byte[2];

            string erg3;
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            string message = "As an alternative to a manual key you can choose any file (eg a jpeg image) as a key file. The file must be at least 7 kbyte. A 1024 bytes big key will be generated. Do you would like to use a key file?";
            string caption = "Load key file";
            string erg4 = "Yes";




            MessageBoxResult result = MessageBox.Show(message, caption, buttons);
            erg3 = result.ToString();


            if (erg3.Equals(erg4))
            {

                // Standarddialog zum Öffnen anzeigen
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "All Files (*.*)|*.*";
                openFileDialog1.Title = "Open a file";

                openFileDialog1.ShowDialog();


                //textBox1.Text = openFileDialog1.FileName;
                schluesseldatei = openFileDialog1.FileName;



                try
                {

                    FileStream keyfile = new FileStream(@schluesseldatei, FileMode.Open, FileAccess.Read);
                    BinaryReader keyreader = new BinaryReader(keyfile);


                    if (keyfile.Length > 7000)
                    {


                        for (long i = 0; i < 2500; i++)
                        {
                            //binWriter3.Write((schreib));
                            x = (byte)(keyreader.ReadByte());



                        }
                        //MessageBox.Show("Bereich erreicht!"); 
                        for (long i = 0; i < 1023; i++)
                        {
                            //binWriter3.Write((schreib));
                            name_der_datei6X[i] = (byte)(keyreader.ReadByte());



                        }

                        for (long i = 0; i < 1023; i++)
                        {
                            //binWriter3.Write((schreib));
                            name_der_datei6X[i] = (byte)(name_der_datei6X[i] ^ ((byte)(keyreader.ReadByte())));



                        }

                        for (long i = 0; i < 1023; i++)
                        {
                            //binWriter3.Write((schreib));
                            name_der_datei6X[i] = (byte)(name_der_datei6X[i] ^ ((byte)(keyreader.ReadByte())));



                        }

                        //MessageBox.Show("Bereich2 erreicht!"); 
                        keyfile.Close();
                        keyreader.Close();



                        textBox3.Password = "";
                        textBox4.Password = "";

                        schluesseldatei_geladen = 1;
                        button6.Background = Brushes.Green;

                        progressBar2.Visibility = Visibility.Hidden;
                        label2.Visibility = Visibility.Visible;
                        label1.Visibility = Visibility.Hidden;




                        label4.Visibility = Visibility.Hidden;
                        //radioButton1.Visibility = Visibility.Hidden;
                        //radioButton2.Visibility = Visibility.Hidden;
                        image5.Visibility = Visibility.Visible;
                        if ((bool)radioButton1.IsChecked)
                        {

                            textBox5.Background = Brushes.Transparent;

                        }
                        else
                        {

                            textBox3.Background = Brushes.Transparent;
                            textBox4.Background = Brushes.Transparent;
                        }



                        /*
                                                     textBox3.Password = "";
                                                     textBox4.Password = "";
                                                     textBox3.Password = ByteArrayToString(schluessel);
                                                     textBox4.Password = ByteArrayToString(schluessel);
                                                      */










                    }
                    else
                    {
                        ShowFg("The file is too small (the file must be at least 7 kbyte).");

                    }

                }



                catch
                {
                    ShowFg("Access not possible!");
                }
            }


        }





        private void textBox4_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {

            if (prozess_laueft == false)
            {
                label5.Foreground = Brushes.Red;
                SetBurningLogo(true);
                // image4 entfernt 2026-05-20 (Tur_effekt2.bmp war V4.1-Overlay)
                prozess_laueft = true;
                dateigroesse1 = textBox1.Text;
                dateigroesse2 = textBox2.Text;

                dateil1 = dateigroesse1.Length;
                dateil2 = dateigroesse2.Length;


                backgroundWorker2.RunWorkerAsync();

            }

        }

        private void image2_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }




        private void flame_aus()
        {


            image2.Visibility = Visibility.Hidden;
            image3.Visibility = Visibility.Visible;


        }

        private void flame_ein()
        {


            image3.Visibility = Visibility.Hidden;
            image2.Visibility = Visibility.Visible;


        }


        internal static byte[] String2Hex(string s)
        {
            byte[] bt = Encoding.GetEncoding(850).GetBytes(s);
            return bt;
        }

        internal static string Hex2String(byte[] bArray)
        {
            //string sTelegram = string.Empty;

            Encoding enc = Encoding.GetEncoding(850);
            string sTelegram = enc.GetString(bArray);
            /*
            foreach (byte b in bArray)
            {
                sTelegram += ((char)Convert.ToInt32(b));
            }*/
            return sTelegram;
        }


        /*
        internal static string Hex2String(byte[] b)

        {
            string st = Encoding.GetEncoding(850).GetBytes(b);
            return st;
        }
        */
        private string ByteArrayToString(byte[] arr)
        {
            System.Text.UTF8Encoding enc2 = new System.Text.UTF8Encoding();
            //System.Text.Encoding enc2 = new System.Text.Encoding();
            // System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc2.GetString(arr);
        }

        private void progressBar2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void radioButton4_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            string message = "Mighty mouse is an ANTI-keylogger function. If you are not sure that a keyboard-logger isn´t installed on this PC (e.g. a public PC in an internet cafe) - then use the mouse to type in the key (or a part of the key). With the mighty mouse function you are able to type in ALL 95 printable ASCII characters: digits (0-9), uppercase letters (A-Z), lowercase letters (a-z), special characters and SPACE. Note: Mighty mouse only protects against simple keyboard-loggers - it does NOT protect against screen-recorders or advanced malware.";
            string caption = "Use mighty mouse?";
            string erg_maus2 = "Yes";

            MessageBoxResult result_maus = MessageBox.Show(message, caption, buttons);
            string erg_maus = result_maus.ToString();
            if (erg_maus.Equals(erg_maus2))
            {
                if (MightyMousePanel != null) MightyMousePanel.Visibility = Visibility.Visible;
            }
            else {
                if (MightyMousePanel != null) MightyMousePanel.Visibility = Visibility.Hidden;
            }
        }

        private void SpecialChar_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null && (b.Tag != null || b.Content != null))
            {
                // Tag hat Vorrang vor Content - erlaubt z.B. Anzeige "SP" mit Wert " "
                string charToadd = b.Tag != null ? b.Tag.ToString() : b.Content.ToString();

                if (passwort_anzeige == 1)
                {
                    textBox5.Text = textBox5.Text + charToadd;
                }
                else
                {
                    textBox3.Password = textBox3.Password + charToadd;
                    textBox4.Password = textBox4.Password + charToadd;
                }
            }
        }

        private void button22_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '0';
            }
            else
            {
                textBox3.Password = textBox3.Password + '0';
                textBox4.Password = textBox4.Password + '0';
            }


        }

        private void button23_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '1';
            }
            else
            {
                textBox3.Password = textBox3.Password + '1';
                textBox4.Password = textBox4.Password + '1';
            }

        }

        private void button24_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '2';
            }
            else
            {
                textBox3.Password = textBox3.Password + '2';
                textBox4.Password = textBox4.Password + '2';
            }

        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '3';
            }
            else
            {
                textBox3.Password = textBox3.Password + '3';
                textBox4.Password = textBox4.Password + '3';
            }

        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '4';
            }
            else
            {
                textBox3.Password = textBox3.Password + '4';
                textBox4.Password = textBox4.Password + '4';
            }

        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '5';
            }
            else
            {
                textBox3.Password = textBox3.Password + '5';
                textBox4.Password = textBox4.Password + '5';
            }

        }

        private void button12_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '6';
            }
            else
            {
                textBox3.Password = textBox3.Password + '6';
                textBox4.Password = textBox4.Password + '6';
            }

        }

        private void button11_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '7';
            }
            else
            {
                textBox3.Password = textBox3.Password + '7';
                textBox4.Password = textBox4.Password + '7';
            }

        }

        private void button13_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '8';
            }
            else
            {
                textBox3.Password = textBox3.Password + '8';
                textBox4.Password = textBox4.Password + '8';
            }

        }

        private void button14_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + '9';
            }
            else
            {
                textBox3.Password = textBox3.Password + '9';
                textBox4.Password = textBox4.Password + '9';
            }

        }

        private void button15_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + 'A';
            }
            else
            {
                textBox3.Password = textBox3.Password + 'A';
                textBox4.Password = textBox4.Password + 'A';
            }

        }

        private void button16_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + 'B';
            }
            else
            {
                textBox3.Password = textBox3.Password + 'B';
                textBox4.Password = textBox4.Password + 'B';
            }

        }

        private void button17_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + 'C';
            }
            else
            {
                textBox3.Password = textBox3.Password + 'C';
                textBox4.Password = textBox4.Password + 'C';
            }

        }

        private void button18_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + 'D';
            }
            else
            {
                textBox3.Password = textBox3.Password + 'D';
                textBox4.Password = textBox4.Password + 'D';
            }

        }

        private void button19_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + 'E';
            }
            else
            {
                textBox3.Password = textBox3.Password + 'E';
                textBox4.Password = textBox4.Password + 'E';
            }

        }

        private void button20_Click(object sender, RoutedEventArgs e)
        {
            if (passwort_anzeige == 1)
            {

                textBox5.Text = textBox5.Text + 'F';
            }
            else
            {
                textBox3.Password = textBox3.Password + 'F';
                textBox4.Password = textBox4.Password + 'F';
            }

        }



    }




}

