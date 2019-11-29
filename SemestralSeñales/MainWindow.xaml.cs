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
        double score = 0;
        double velocidadobjetos = 0;
        const int umbral = 400;
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
        List<Piedras> monedas = new List<Piedras>();
        List<Piedras> carreteras = new List<Piedras>();
        List<Piedras> tierras = new List<Piedras>();
        List<Piedras> montanas = new List<Piedras>();


        double velocidadcarretera;

        double velocidadtierra;

        double velocidadmontana;




        public MainWindow()
        {
            InitializeComponent();
            stopwatch = new Stopwatch();
            stopwatch.Start();
            tiempoAnterior = stopwatch.Elapsed;

            monedas.Add(new Piedras(Moneda));
            monedas.Add(new Piedras(Moneda1));
            monedas.Add(new Piedras(Moneda3));
            monedas.Add(new Piedras(Moneda5));

            piedras.Add(new Piedras(imgPiedra1));
            piedras.Add(new Piedras(imgPiedra2));
            piedras.Add(new Piedras(imgPiedra3));
            piedras.Add(new Piedras(imgPiedra4));


            carreteras.Add(new Piedras(LaCarreta1));
            carreteras.Add(new Piedras(LaCarreta2));

            montanas.Add(new Piedras(LaMontana1));
            montanas.Add(new Piedras(LaMontana2));


            tierras.Add(new Piedras(LaTierra1));
            tierras.Add(new Piedras(LaTierra2));

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
            lblHertz.Text = frecuenciaFundamental.ToString("N") + "Hz";
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
            if(bochomain.PosicionY <= 40 || bochomain.PosicionY >= 310)
            {
                bochomain.CambiarDireccion(Bocho.Direccion.Estatico);

            }

           foreach (Piedras carretera in carreteras)
            {
                carretera.Mover(deltaTime);
                carretera.Velocidad += velocidadcarretera * deltaTime;
                if (carretera.PosicionX <= -1174)
                {
                    carretera.PosicionX = 786;
                }
            }
            foreach (Piedras tierra in tierras)
            {
                tierra.Mover(deltaTime);
                tierra.Velocidad += velocidadtierra * deltaTime;
                if (tierra.PosicionX <= -1180)
                {
                    tierra.PosicionX = 800;
                }
            }
            foreach (Piedras montana in montanas)
            {
                montana.Mover(deltaTime);
                montana.Velocidad += velocidadmontana * deltaTime;
                if (montana.PosicionX <= -1180)
                {
                    montana.PosicionX = 1200;
                }
            }
            foreach (Piedras piedra in piedras)
                {
                piedra.Mover(deltaTime);
                piedra.Velocidad += velocidadobjetos * deltaTime;
                if(piedra.PosicionX <= -10)
                {
                    Random rnd = new Random();
                    double rango = rnd.Next(40, 310);
                    piedra.PosicionY = rango;
                    piedra.PosicionX = 840;
                }
                if (piedra.PosicionX + piedra.Imagen.Width >= bochomain.PosicionX &&
                    piedra.PosicionX <= bochomain.PosicionX + bochomain.Imagen.Width &&
                    piedra.PosicionY + piedra.Imagen.Height >= bochomain.PosicionY &&
                    piedra.PosicionY <= bochomain.PosicionY + bochomain.Imagen.Height)
                {
                    estadoActual = EstadoJuego.Gameover;
                    CanvasGamePlay.Visibility = Visibility.Collapsed;
                    Score.Content = score.ToString("N") + "$";
                    CanvasExit.Visibility = Visibility.Visible;
                   
                }
                }
            foreach (Piedras moneda in monedas)
            {
                if (moneda.PosicionX <= -10)
                {
                    Random rnd = new Random();
                    float rango = rnd.Next(40, 310);
                    moneda.PosicionY = rango;
                    moneda.PosicionX = 840;
                }
                moneda.Mover(deltaTime);
                moneda.Velocidad += velocidadobjetos * deltaTime;

                if (moneda.PosicionX + moneda.Imagen.Width >= bochomain.PosicionX &&
            moneda.PosicionX <= bochomain.PosicionX + bochomain.Imagen.Width &&
            moneda.PosicionY + moneda.Imagen.Height >= bochomain.PosicionY &&
            moneda.PosicionY <= bochomain.PosicionY + bochomain.Imagen.Height)
                {
                    score += 100;
                    Random rnd = new Random();
                    float rango = rnd.Next(40, 310);
                    moneda.PosicionY = rango;
                    moneda.PosicionX = 840;
                }
            }
            
            Dinero.Text = score.ToString("N") + "$";
            
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
                velocidadobjetos = 5;
                velocidadcarretera = 30;
                velocidadtierra = 20;
                velocidadmontana = 1;

            }
            if (Medio.IsChecked == true)
            {
                BitmapImage carro2 = new BitmapImage(new Uri("Carro1.png", UriKind.Relative));
                bochomain.Imagen.Source = carro2;
                velocidadobjetos = 10;
                velocidadcarretera = 60;
                velocidadtierra = 50;
                velocidadmontana = 2;

            }
            if (Difizcil.IsChecked == true)
            {
                BitmapImage carro3 = new BitmapImage(new Uri("c.png", UriKind.Relative));
                bochomain.Imagen.Source = carro3;
                velocidadobjetos = 15;
                velocidadcarretera = 90;
                velocidadtierra = 60;
                velocidadmontana = 3;


            }
            if (Imposibruu.IsChecked == true)
            {
                BitmapImage carro4 = new BitmapImage(new Uri("carrito.png", UriKind.Relative));
                bochomain.Imagen.Source = carro4;
                velocidadobjetos = 20;
                velocidadcarretera = 120;
                velocidadtierra = 90;
                velocidadmontana = 4;


            }
            foreach (Piedras piedra in piedras)
            {
                piedra.CambiarDireccion(Piedras.Direccion.Izquierda);
            }

            foreach (Piedras moneda in monedas)
            {
                moneda.CambiarDireccion(Piedras.Direccion.Izquierda);
            }
            foreach (Piedras carretera in carreteras)
            {
                carretera.CambiarDireccion(Piedras.Direccion.Izquierda);
            }
            foreach (Piedras tierra in tierras)
            {
                tierra.CambiarDireccion(Piedras.Direccion.Izquierda);
            }
            foreach (Piedras montana in montanas)
            {
                montana.CambiarDireccion(Piedras.Direccion.Izquierda);
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
