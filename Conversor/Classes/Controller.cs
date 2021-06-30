using System;
using System.Collections.Generic;
using System.Linq;

namespace Conversor.Clases
{
    public class Controller
    {
        #region Public Methods

        #endregion

        public static List<(string descripcion, object contenido, int posicion)> GetValueOfExtractions<T>
            (string line, List<T> listExtractionsToDo, List<(string descripcion,
                int[] links, string separador)> Links = null, bool printLinksOnly = false) where T : IExtraction
        {
            // Generamos una lista a rellenar con los resultados.
            var listWithResults = new List<(string descripcion, object contenido, int posicion)>();

            // Obtenemos las extracciones validas de nuestra lista de extracciones.
            listExtractionsToDo = listExtractionsToDo.Where(x => ValidateExtraction(x) != null).ToList();

            // Hacemos que cada extraccion lleve a cabo su tarea.
            listExtractionsToDo.ForEach(x => x.Extract(line));

            // Generamos la lista de resultados obtenidos de las extracciones.
            listExtractionsToDo.ForEach(x => listWithResults.Add((x.Description, x.ToString(), x.Order)));

            // Consultamos si hay alguna union por hacerse
            if (Links != null)
            {
                var listOfLinksResults = new List<(string descripcion, object contenido, int posicion)>();
                int i = printLinksOnly ? 0 : listWithResults.Count - 1;
                Links.ForEach(x =>
                {
                    List<string> extractionsToLink = new List<string>();
                    // Agregamos a una lista las extracciones en el orden que deseamos unirlas
                    foreach (int IndexOfExtraction in x.links)
                        extractionsToLink.Add(listExtractionsToDo[IndexOfExtraction].ToString());
                    // Unimos las extracciones con el separador indicado
                    var resultOfUnion = string.Join(x.separador, extractionsToLink.ToArray());
                    // finalmente agregamos a la lista de resultados la union de las extracciones
                    listOfLinksResults.Add((x.descripcion, resultOfUnion, ++i));
                });
                if (printLinksOnly)
                    return listOfLinksResults;

                else
                    listWithResults.AddRange(listOfLinksResults);
            }
            return listWithResults;
        }
        private static IExtraction ValidateExtraction(IExtraction extraction)
        {
            if (extraction.IsValid)
                return extraction;
            return (IExtraction)default;
        }

        #region Intefaces
        public interface IExtraction
        {
            public string ErrorMessage { get; set; }
            public bool IsValid { get; set; }
            public string Description { get; set; }
            public int InitialPosition { get; set; }
            public bool Selected { get; set; }
            public int Length { get; set; }
            public int Order { get; set; }
            public bool Extract(string line);
            public void SetError(string msg);
            public string ToString();
        }

        #endregion

        #region Classes
        public sealed class Extraction<T> : IExtraction
        {
            #region Constructors
            public Extraction(int initialPosition, int length, string descripcion, int order) : base()
            {
                InitialPosition = initialPosition;
                Length = length;
                Description = descripcion;
                IsValid = true;
                Order = order;
                Validate();
            }
            #endregion

            #region Properties
            public string ErrorMessage { get; set; }
            public bool IsValid { get; set; }
            public int InitialPosition { get; set; }
            public int Length { get; set; }
            public int Order { get; set; }
            public string Description { get; set; }
            public string Result { get; set; }
            public bool Selected { get; set; }
            #endregion

            #region Methods
            public string GetDescription() => Description;
            public string GetResult() => Result;
            public bool GetValidation() => IsValid;
            public string GetError() => ErrorMessage;
            public void SetError(string msg)
            {
                ErrorMessage = msg;
                IsValid = string.IsNullOrEmpty(ErrorMessage);
            }
            private bool Validate()
            {
                SetError(InitialPosition < 0 ? "La posición inicial debe ser mayor que cero." :
                               Length < 0 ? "La longitud debe ser mayor que cero." :
                               string.IsNullOrEmpty(Description) ? "La descripción no puede estar vacía." :
                               string.Empty);
                return IsValid;
            }
            public bool Extract(string line)
            {
                if (string.IsNullOrEmpty(Result))
                {
                    dynamic result = line.Substring(InitialPosition, Length);
                    switch (typeof(T).Name)
                    {
                        case "Int32":
                            result = int.Parse(result);
                            break;

                        case "Int64":
                            result = long.Parse(result);
                            break;

                        case "Decimal":
                            result = (decimal.Parse(result) / 100);
                            break;

                        case "Double":
                            result = (double.Parse(result) / 100);
                            break;

                        case "String":
                            break;

                        default:
                            return false;
                    }
                    Result = result.ToString();
                }
                return true;
            }
            public override string ToString() => string.IsNullOrEmpty(Result) ? "Debe realizar la extracción primero." : Result;
            #endregion
        }
        #endregion
    }
}
