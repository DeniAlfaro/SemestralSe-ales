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
    ///
    public partial class MainWindow : Window
    {
        double velocidadobjetos = 0;
        const int umbral = 1000;
        WaveIn waveIn; //conexion con microfono
        WaveFormat formato; //formato de audio
        enum EstadoJuego { Menu, Gameplay, Gameover };
        EstadoJuego estadoActual = EstadoJuego.Menu;
        Bocho bochomain;
        double calando = 0;
        Stopwatch stopwatch;
        TimeSpan tiempoAnterior;
        public float frecuenciaFundamental;
        List<Piedras> piedras = new List<Piedras>();
        public MainWindow()
        {
            InitializeComponent();
            stopwatch = new Stopwatch();
            stopwatch.Start();
            tiempoAnterior = stopwatch.Elapsed;

            piedras.Add(new Piedras(imgPiedra1));
            piedras.Add(new Piedras(imgPiedra2));
            piedras.Add(new Piedras(imgPiedra3));
            piedras.Add(new Piedras(imgPiedra4));
            piedras.Add(new Piedras(imgCono));

            bochomain = new Bocho(imgCarrito);

            bochomain.CambiarDireccion(Bocho.Direccion.Arriba);
            tiempoAnterior = stopwatch.Elapsed;
        }
        public void cicloPrincipal()
        {
            while (estadoActual == EstadoJuego.Gameplay )
            {
                Dispatcher.Invoke(actualizar);

            }
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
            frecuenciaFundamental = (float)(indiceValorMaximo * formato.SampleRate) / (float)(valoresAbsolutos.Length);
         
        }

        void actualizar()
        {
            TimeSpan tiempoActual = stopwatch.Elapsed;
            double deltaTime = tiempoActual.TotalSeconds - tiempoAnterior.TotalSeconds;
       
            bochomain.Mover(deltaTime);
            bochomain.Velocidad += 10 * deltaTime;
            calando += deltaTime;
            
            if (frecuenciaFundamental >= umbral)
            {
                bochomain.CambiarDireccion(Bocho.Direccion.Arriba);
            }
            if (frecuenciaFundamental <= umbral)
            {
                bochomain.CambiarDireccion(Bocho.Direccion.Abajo);
            }
            //colisiones
            //faltan colisiones con baches (que no los he puesto porque no los puedo poner xd) y colision con el cono
            foreach (Piedras piedra in piedras)
                {
                    double xCarrito = Canvas.GetLeft(imgCarrito);
                    double xPiedra = Canvas.GetLeft(piedra.Imagen);
                    double yCarrito = Canvas.GetTop(imgCarrito);
                    double yPiedra = Canvas.GetTop(piedra.Imagen);
                piedra.Mover(deltaTime);
                piedra.Velocidad += velocidadobjetos * deltaTime;
                piedra.CambiarDireccion(Piedras.Direccion.Izquierda);

                    if (xPiedra + piedra.Imagen.Width >= xCarrito && xPiedra <= xCarrito + imgCarrito.Width &&
                        yPiedra + piedra.Imagen.Height >= yCarrito && yPiedra <= yCarrito + imgCarrito.Height)
                    {
                    lblHertz.Text = "Lo tocoo";
                    
                        estadoActual = EstadoJuego.Gameover;
                        CanvasGamePlay.Visibility = Visibility.Collapsed;
                        CanvasExit.Visibility = Visibility.Visible; //cambiarlo a pantalla gameover(que todavia no está hecha jeje)
                        
                    }
                }
                tiempoAnterior = tiempoActual;           
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            CanvasGamePlay.Focus();
            ThreadStart threadStart = new ThreadStart(cicloPrincipal);
            Thread thread = new Thread(threadStart);
            thread.Start();
            estadoActual = EstadoJuego.Gameplay;
            CanvasMenu.Visibility = Visibility.Collapsed;
            CanvasGamePlay.Visibility = Visibility.Visible;
            if(Facil.IsChecked == true)
            {
                BitmapImage carro1 = new BitmapImage(new Uri("Carro3.png", UriKind.Relative));
                bochomain.Imagen.Source = carro1;
                velocidadobjetos = 7;
            }
            if (Medio.IsChecked == true)
            {
                BitmapImage carro2 = new BitmapImage(new Uri("Carro1.png", UriKind.Relative));
                bochomain.Imagen.Source = carro2;
                velocidadobjetos = 12;
            }
            if (Difizcil.IsChecked == true)
            {
                BitmapImage carro3 = new BitmapImage(new Uri("c.png", UriKind.Relative));
                bochomain.Imagen.Source = carro3;
                velocidadobjetos = 20;
            }
            if (Imposibruu.IsChecked == true)
            {
                BitmapImage carro4 = new BitmapImage(new Uri("carrito.png", UriKind.Relative));
                bochomain.Imagen.Source = carro4;
                velocidadobjetos = 25;
            }
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
            System.Windows.Application.Current.Shutdown();
        }

    }
}
