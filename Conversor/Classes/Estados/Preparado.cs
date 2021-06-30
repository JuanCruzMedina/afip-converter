using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaState.Estados
{
    class Preparado : EstadoProceso
    {
        public override void Interrumpir()
        {
            this._context.Transitar(new Interrumpido());
        }

        public override void Validar()
        {
            this._context.Transitar(new Validado());
        }

        public override void Exportar()
        {
            throw new NotImplementedException();
        }

        public override void Finalizar()
        {
            throw new NotImplementedException();
        }

        public override void Previsualizar()
        {
            throw new NotImplementedException();
        }

        public override void Iniciar()
        {
            throw new NotImplementedException();
        }
        private bool ValidFields(out string message, out string extractionPath)
        {
            extractionPath = message = string.Empty;
            try
            {
                if (this._context.FilePath == string.Empty)
                    message = ("Debes seleccionar un archivo a corregir.");
                else
                    extractionPath = !this._context.ExtractionDefault ? this._context.DefaultPath : System.IO.Path.GetDirectoryName(this._context.FilePath);


                if (extractionPath == string.Empty && !this._context.ExtractionDefault)
                    message = ("Debes seleccionar una ruta de exportación.");

                else if (!PathIsValid(this._context.FilePath))
                    message = ("El formato del archivo no es válido.");

                else return true;

                return false;
            }
            catch (Exception ex)
            {
                message = ("Ocurrió una inconsistencia durante la corrección del archivo.\n" + ex.Message);
                return false;
            }
        }
        private bool PathIsValid(string filePath)
        {
            bool result = false;
            switch (System.IO.Path.GetExtension(filePath))
            {
                case (".txt"):
                    result = true;
                    break;
            }
            return result;
        }

        public override void Next()
        {
            string msg, defaultPath;
            if (ValidFields(out msg, out defaultPath))
                Validar();

            else
            {
                this._context.mensaje = msg;
                Interrumpir();
            }

        }
    }
}
