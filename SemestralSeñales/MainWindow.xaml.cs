using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;

using NAudio;
using NAudio.Wave;
using NAudio.Dsp;
namespace SemestralSeñales
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int umbral = 800;
        WaveIn waveIn; //conexion con microfono
        WaveFormat formato; //formato de audio
        enum EstadoJuego { Menu, Gameplay, Gameover };
        EstadoJuego estadoActual = EstadoJuego.Menu;

        enum Carril { Arriba, Abajo, Ninguna };
        Carril CarrilActual = Carril.Arriba;

        double velocidadCarrito = 70;

        double cambioDeCarrilPixeles = 50;

        double velocidadSalto = 5;
        double cantidadSaltoCambioDeCarril = 0;
        public MainWindow()
        {
            InitializeComponent();

            // 1. establecer instrucciones
            ThreadStart threadStart = new ThreadStart(actualizar);
            // 2. inicializar el Thread
            Thread threadMover = new Thread(threadStart);
            // 3. ejecutar el Thread
            threadMover.Start();
        }


        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesGrabados = e.BytesRecorded;

            int numMuestras = bytesGrabados / 2;

            int exponente = 0;
            int numBits = 0;
            do
            {
                exponente++;
                numBits = (int)Math.Pow(2, exponente);
            } while (numBits < numMuestras);
            exponente -= 1;

            numBits = (int)Math.Pow(2, exponente);
            Complex[] muestrasComplejas = new Complex[numBits];

            for (int i = 0; i < bytesGrabados; i += 2)
            {
                short muestra = (short)(buffer[i + 1] << 8 | buffer[i]);
                float muestra32bits = (float)muestra / 32768.0f;
                if (i / 2 < numBits)
                {
                    muestrasComplejas[i / 2].X = muestra32bits;
                }
            }
            FastFourierTransform.FFT(true, exponente, muestrasComplejas);
            float[] valoresAbsolutos = new float[muestrasComplejas.Length];

            for (int i = 0; i < muestrasComplejas.Length; i++)
            {
                valoresAbsolutos[i] = (float)Math.Sqrt((muestrasComplejas[i].X + muestrasComplejas[i].X)
                    + (muestrasComplejas[i].Y * muestrasComplejas[i].Y));

            }
            var mitadValoresAbsolutos = valoresAbsolutos.Take(valoresAbsolutos.Length / 2).ToList();
            int indiceValorMaximo = mitadValoresAbsolutos.IndexOf(mitadValoresAbsolutos.Max());
            float frecuenciaFundamental = (float)(indiceValorMaximo * formato.SampleRate) / (float)(valoresAbsolutos.Length);

            lblHertz.Text = frecuenciaFundamental.ToString("N") + "H";

            if(frecuenciaFundamental > umbral)
            {
                CarrilActual = Carril.Arriba;
            } else
            {
                CarrilActual = Carril.Abajo;
            }
        }


        void moverCarrito(TimeSpan deltaTime)
        {
            if (estadoActual == EstadoJuego.Gameplay)
            {
                double topCarritoActual = Canvas.GetTop(imgCarrito);
                switch (CarrilActual)
                {

                    case Carril.Arriba:
                        double cantidadMovimiento = ((cambioDeCarrilPixeles * deltaTime.TotalSeconds) * velocidadSalto);
                        cantidadSaltoCambioDeCarril += cantidadMovimiento;
                        if (cantidadSaltoCambioDeCarril <= cambioDeCarrilPixeles)
                        {
                            Canvas.SetTop(imgCarrito, topCarritoActual - cantidadMovimiento);
                        }
                        else
                        {
                            CarrilActual = Carril.Abajo;
                        }
                        break;
                    case Carril.Abajo:
                        double nuevaPosicion = topCarritoActual + (velocidadCarrito * deltaTime.TotalSeconds);
                        if (nuevaPosicion + imgCarrito.Width <= 450)
                        {
                            Canvas.SetTop(imgCarrito, nuevaPosicion);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        void actualizar()
        {

        }

        /*void actualizar()
        {

            while (true)
            {
                Dispatcher.Invoke(
                () =>
                {
                    var tiempoActual = stopwatch.Elapsed;
                    var deltaTime = tiempoActual - tiempoAnterior;
                    if (estadoActual == EstadoJuego.Gameplay)
                    {

                        miCanvas.Focus();
                        moverCarrito(deltaTime);

                        //colisiones
                        foreach (Popotes popote in popotes)
                        {
                            double xTurtle = Canvas.GetLeft(imgTurtle);
                            double xPopotes = Canvas.GetLeft(popote.Imagen);
                            double yTurtle = Canvas.GetTop(imgTurtle);
                            double yPopotes = Canvas.GetTop(popote.Imagen);

                            if (xPopotes + popote.Imagen.Width >= xTurtle && xPopotes <= xTurtle + imgTurtle.Width &&
                                yPopotes + popote.Imagen.Height >= yTurtle && yPopotes <= yTurtle + imgTurtle.Height)
                            {
                                estadoActual = EstadoJuego.Gameover;
                                miCanvas.Visibility = Visibility.Collapsed;
                                canvasGameOver.Visibility = Visibility.Visible;
                            }
                        }

                        if (score >= 200)
                        {
                            lblNivel1.Visibility = Visibility.Collapsed;
                            lblNivel2.Visibility = Visibility.Visible;
                            imgFondo2.Visibility = Visibility.Collapsed;
                            imgFondo1.Visibility = Visibility.Visible;
                        }

                        if (score >= 350)
                        {
                            lblNivel2.Visibility = Visibility.Collapsed;
                            lblNivel3.Visibility = Visibility.Visible;
                            imgFondo1.Visibility = Visibility.Collapsed;
                            imgFondo3.Visibility = Visibility.Visible;
                        }

                        tiempoAnterior = tiempoActual;
                    }
                });

            }
        }*/

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //inicializar la conexion
            waveIn = new WaveIn();

            //establecer el formato
            waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
            formato = waveIn.WaveFormat;

            //duracion del buffer
            waveIn.BufferMilliseconds = 500;

            //con que funcion respondemos cuando se llena el buffer
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveIn.StartRecording();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            waveIn.StopRecording();
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
