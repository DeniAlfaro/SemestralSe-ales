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


namespace SemestralSeñales
{
    class Bocho
    {
        public Image Imagen { get; set; }

        public enum Direccion { Izquierda, Derecha, Arriba, Abajo, Estatico };
        Direccion DireccionActual { get; set; }

        double PosicionX { get; set; }
        double PosicionY { get; set; }

        public double Velocidad { get; set; }
        public Bocho(Image imagen)
        {
            Imagen = imagen;

            PosicionX = Canvas.GetLeft(imagen);
            PosicionY = Canvas.GetTop(imagen);

            DireccionActual = Direccion.Estatico;

            Velocidad = 1;

        }
        public void CambiarDireccion(Direccion nuevaDireccion)
        {
            DireccionActual = nuevaDireccion;

        }
        public void Mover(double deltaTime)
        {
            switch (DireccionActual)
            {
                case Direccion.Abajo:
                    PosicionY += Velocidad * deltaTime;

                    break;
                case Direccion.Arriba:
                    PosicionY -= Velocidad * deltaTime;

                    break;
                default:
                    break;
            }
            Canvas.SetLeft(Imagen, PosicionX);
            Canvas.SetTop(Imagen, PosicionY);
        }
        }
}
