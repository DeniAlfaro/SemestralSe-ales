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
        double cantidadSaltoCambioDeCarril = 5;

        Stopwatch stopwatch;
        TimeSpan tiempoAnterior;

        List<Piedras> piedras = new List<Piedras>();
        public MainWindow()
        {
            InitializeComponent();
            CanvasGamePlay.Focus();

            stopwatch = new Stopwatch();
            stopwatch.Start();
            tiempoAnterior = stopwatch.Elapsed;

            piedras.Add(new Piedras(imgPiedra1));
            piedras.Add(new Piedras(imgPiedra2));

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

            while (true)
            {
                Dispatcher.Invoke(
                () =>
                {
                    var tiempoActual = stopwatch.Elapsed;
                    var deltaTime = tiempoActual - tiempoAnterior;
                    if (estadoActual == EstadoJuego.Gameplay)
                    {

                        CanvasGamePlay.Focus();
                        moverCarrito(deltaTime);

                        //colisiones
                        //faltan colisiones con baches (que no los he puesto porque no los puedo poner xd) y colision con el cono
                        foreach (Piedras piedra in piedras)
                        {
                            double xCarrito = Canvas.GetLeft(imgCarrito);
                            double xPiedra = Canvas.GetLeft(piedra.Imagen);
                            double yCarrito = Canvas.GetTop(imgCarrito);
                            double yPiedra = Canvas.GetTop(piedra.Imagen);

                            if (xPiedra + piedra.Imagen.Width >= xCarrito && xPiedra <= xCarrito + imgCarrito.Width &&
                                yPiedra + piedra.Imagen.Height >= yCarrito && yPiedra <= yCarrito + imgCarrito.Height)
                            {
                                estadoActual = EstadoJuego.Gameover;
                                CanvasGamePlay.Visibility = Visibility.Collapsed;
                                CanvasExit.Visibility = Visibility.Visible; //cambiarlo a pantalla gameover(que todavia no está hecha jeje)
                            }
                        }

                        tiempoAnterior = tiempoActual;
                    }
                });

            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            estadoActual = EstadoJuego.Gameplay;
            CanvasMenu.Visibility = Visibility.Collapsed;
            CanvasGamePlay.Visibility = Visibility.Visible;
        
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
