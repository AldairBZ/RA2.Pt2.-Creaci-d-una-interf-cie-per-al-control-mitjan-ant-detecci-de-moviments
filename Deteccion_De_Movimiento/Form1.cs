using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum; // <--- Asegúrate de que esta línea esté presente
namespace Deteccion_Movimiento
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture; // Captura de la cámara
        private Mat _frameAnterior = new Mat(); // Para comparar frames
        private bool _isCapturing = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            MessageBox.Show("¡Botón pulsado!");
            if (_capture == null)
            {
                _capture = new VideoCapture(0, VideoCapture.API.DShow);
                // El evento ImageGrabbed se activa cada vez que la cámara tiene un frame nuevo
                _capture.ImageGrabbed += ProcessFrame;
            }
            _capture.Start();
            _isCapturing = true;

            if (_capture.IsOpened)
            {
                MessageBox.Show("La cámara se abrió correctamente");
            }
            else
            {
                MessageBox.Show("No se pudo abrir la cámara. Revisa la conexión en la VM.");
            }
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (_isCapturing && _capture != null)
            {
                Mat frameActual = new Mat();

                // "Retrieve" saca la foto de la cámara y la mete en frameActual
                _capture.Retrieve(frameActual);

                if (!frameActual.IsEmpty)
                {
                    // LÓGICA PARA LA DETECCIÓN DE MOVIMIENTO
                    if (!_frameAnterior.IsEmpty)
                    {
                        Mat diff = new Mat();
                        // Comparamos el frame actual con el anterior
                        CvInvoke.AbsDiff(frameActual, _frameAnterior, diff);
                        CvInvoke.CvtColor(diff, diff, ColorConversion.Bgr2Gray);
                        CvInvoke.Threshold(diff, diff, 25, 255, ThresholdType.Binary);

                        // Contamos píxeles que han cambiado
                        int m = CvInvoke.CountNonZero(diff);

                        // Si hay movimiento (más de 500 píxeles), ponemos el fondo en rojo
                        this.Invoke(new Action(() => {
                            if (m > 3000)
                            {
                                this.BackColor = Color.Red;
                            }
                            else
                            {
                                this.BackColor = SystemColors.Control;
                            }
                        }));
                    }

                    // Guardamos el frame actual para compararlo en la siguiente vuelta
                    frameActual.CopyTo(_frameAnterior);

                    // DIBUJAMOS LA IMAGEN EN EL PICTUREBOX
                    pbVideo.Invoke(new Action(() => {
                        // Usamos la extensión para convertir Mat a algo que el PictureBox entienda
                        pbVideo.Image = Emgu.CV.BitmapExtension.ToBitmap(frameActual);
                    }));
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_capture != null)
            {
                _capture.Stop();
                _isCapturing = false;
            }
        }
    }
}