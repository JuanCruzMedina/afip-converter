using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaState.Estados
{
    class Finalizado : EstadoProceso
    {
        public override void Finalizar()
        {
            System.Windows.Forms.MessageBox.Show("¡La corrección del archivo se llevo a cabo exitosamente!");
            this._context.fin = true;
        }
        public override void Next()
        {
            System.Windows.Forms.MessageBox.Show("¡La corrección del archivo se llevo a cabo exitosamente!");
            this._context.fin = true;
        }

        public override void Iniciar()
        {
            throw new NotImplementedException();
        }

        public override void Interrumpir()
        {
            throw new NotImplementedException();
        }

        public override void Exportar()
        {
            throw new NotImplementedException();
        }

        public override void Previsualizar()
        {
            throw new NotImplementedException();
        }

        public override void Validar()
        {
            throw new NotImplementedException();
        }
    }
}
