using Microsoft.AspNetCore.Mvc;
using storageFirebase.Models;
using System.Diagnostics; 
using Firebase.Storage;
using Firebase.Auth; 

namespace storageFirebase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;        

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Metodo que sube el archivo a Firebase
        /// </summary>
        /// <param name="archivo"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SubirArchivo(IFormFile archivo)
        {
            //Leemos el archivo indicado
            Stream archivoASubir = archivo.OpenReadStream();

            //Configuramos la conexion hacia firebase
            string email = "asp@mail.com";  //Correo para autenticar en firebase
            string clave = "delunoalnueve";  //Contraseña para autenticar en firebase
            string ruta = "storagefiles-5c52d.appspot.com";   //Lugar donde se guardan los archivos
            string api_key = "AIzaSyB8QPVFCvNmZVOKS6cpCVOpxtaZ1X3F3Jk";//Identificador del proyecto firebase a utilizar en el proyecto MVC

            var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));

            var autenticarFireBase = await auth.SignInWithEmailAndPasswordAsync(email, clave);

            var cancellation = new CancellationTokenSource();
            var tokenUser = autenticarFireBase.FirebaseToken;

            var tareaCargarArchivo = new FirebaseStorage( ruta,  
                                                            new FirebaseStorageOptions
                                                            {
                                                                AuthTokenAsyncFactory = () => Task.FromResult(tokenUser), ThrowOnCancel = true
                                                            }
                                                        ).Child("Arhivos")
                                                        .Child(archivo.FileName)
                                                        .PutAsync(archivoASubir, cancellation.Token);

            var urlArchivoCargado = await tareaCargarArchivo;

            HttpContext.Session.SetString("urlArchivoCargado", urlArchivoCargado);

            return RedirectToAction("VerImagen");

        }

        public IActionResult VerImagen()
        {
            ViewData["imagen"] = HttpContext.Session.GetString("urlArchivoCargado");
            return View();
        }
    }
}