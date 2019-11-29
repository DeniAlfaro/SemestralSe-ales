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
    public class Piedras
    {
        public Image Imagen { get; set; }
        public enum Direccion { Izquierda, Estatico };
        Direccion DireccionActual { get; set; }

        public double PosicionX { get; set; }
        public double PosicionY { get; set; }

        public double Velocidad { get; set; }

        public Piedras(Image imagen)
        {
            Imagen = imagen;
            PosicionX = Canvas.GetLeft(imagen);
            PosicionY = Canvas.GetTop(imagen);

            DireccionActual = Direccion.Estatico;

            Velocidad = 20;
        }
        public void CambiarDireccion(Direccion nuevaDireccion)
        {
            DireccionActual = nuevaDireccion;

        }
        public void Mover(double deltaTime)
        {
            switch (DireccionActual)
            {
                case Direccion.Izquierda:
                    PosicionX -= Velocidad * deltaTime;

                    break;
  
                default:
                    break;
            }
            Canvas.SetLeft(Imagen, PosicionX);
            Canvas.SetTop(Imagen, PosicionY);
        }
    }
}
