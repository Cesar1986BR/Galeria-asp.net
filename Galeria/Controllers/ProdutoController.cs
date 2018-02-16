using Galeria.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Galeria.Controllers
{
    public class ProdutoController : Controller
    {
        // GET: Produto
        public ActionResult Index()
        {
            List<Produto> listaProdutos;
            using (Db db = new Db())
            {
                listaProdutos = db.Produto.ToList();

            }
            return View(listaProdutos);
        }

        public ActionResult AddProduto(ProdutoVM model)
        {
            using (Db db = new Db())
            {
                model = new ProdutoVM();
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduto(ProdutoVM model, HttpPostedFileBase file)
        {
            //  model = new ProdutoVM();

            if (!ModelState.IsValid)
            {

                return View(model);
            }

            int id;
            using (Db db = new Db())
            {
                db.Produto.Add(model.Produto);
                db.SaveChanges();

                id = model.Produto.Id;
            }
            TempData["SM"] = "Você adicionou um produto!";

            var diretorio = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var caminho1 = Path.Combine(diretorio.ToString(), "Produto");
            var caminho2 = Path.Combine(diretorio.ToString(), "Produto\\" + id.ToString());
            var caminho3 = Path.Combine(diretorio.ToString(), "Produto\\" + id.ToString() + "\\Thumbs");
            var caminho4 = Path.Combine(diretorio.ToString(), "Produto\\" + id.ToString() + "\\Gallery");
            var caminho5 = Path.Combine(diretorio.ToString(), "Produto\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(caminho1))
                Directory.CreateDirectory(caminho1);

            if (!Directory.Exists(caminho2))
                Directory.CreateDirectory(caminho2);

            if (!Directory.Exists(caminho3))
                Directory.CreateDirectory(caminho3);

            if (!Directory.Exists(caminho4))
                Directory.CreateDirectory(caminho4);

            if (!Directory.Exists(caminho5))
                Directory.CreateDirectory(caminho5);


            if (file != null && file.ContentLength > 0)
            {
                string extensao = file.ContentType.ToLower();

                if (extensao != "image/jpg" &&
                    extensao != "image/jpeg" &&
                    extensao != "image/pjpg" &&
                    extensao != "image/gif" &&
                    extensao != "image/x-png" &&
                    extensao != "image/png")
                {

                    ModelState.AddModelError("", "Erro ao inserir foto, formato incorreto");
                    return View(model);

                }

                string nomedaimagem = file.FileName;

                using (Db db = new Db())
                {
                    Produto prod = db.Produto.Find(id);
                    prod.ImagemNome = nomedaimagem;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", caminho2, nomedaimagem);
                var path2 = string.Format("{0}\\{1}", caminho3, nomedaimagem);

                file.SaveAs(path);


                WebImage img = new WebImage(file.InputStream);
                img.Resize(300, 300);
                img.Save(path2);
            }


            return RedirectToAction("AddProduto");
        }

        public ActionResult EditarProduto(int id)
        {
            ProdutoVM model;
            using (Db db = new Db())
            {
                Produto produto = db.Produto.Find(id);
                model = new ProdutoVM
                {
                    Produto = produto
                };


                model.Galeria = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Produto/" + id + "/Gallery/Thumbs"))
                                         .Select(fn => Path.GetFileName(fn));
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Editarproduto(ProdutoVM model, HttpPostedFileBase file)
        {
            // Get produto id
            int id = model.Produto.Id;

            // select galeria images

            model.Galeria = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Produto/" + id + "/Gallery/Thumbs"))
                                                .Select(fn => Path.GetFileName(fn));

            // verifica modelo stado
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // tem certeza que nome é unico
            using (Db db = new Db())
            {
                if (db.Produto.Where(x => x.Id != id).Any(x => x.Nome == model.Produto.Nome))
                {
                    ModelState.AddModelError("", "Nome do produto já em uso");
                    return View(model);
                }
            }

            // Atualiza produto
            using (Db db = new Db())
            {
                db.Entry(model.Produto).State = EntityState.Modified;
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "Produto editado com sucesso";

            #region Image Upload

            // veriifca por file upload
            if (file != null && file.ContentLength > 0)
            {

                // Get extensao
                string ext = file.ContentType.ToLower();

                // Verica extensao
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Erro ao enviar imagem - formato incorreto");
                        return View(model);
                    }
                }

                // configura diretorio para o upload
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Produto\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Produto\\" + id.ToString() + "\\Thumbs");

                //deleta o arquivo do diretorio

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                // Salva imagem nome

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    Produto produto = db.Produto.Find(id);
                    produto.ImagemNome = imageName;

                    db.SaveChanges();
                }

                // Sala original e miniatura images

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            // Redireciona
            return RedirectToAction("Editarproduto");
        }



        [HttpPost]
        public ActionResult SalvaGaleriaImagens(int id)
        {
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];


                if(file != null && file.ContentLength > 0)
                {
                    // configura um diretorio
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Produto\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Produto\\" + id.ToString() + "\\Gallery\\Thumbs");

                    // configura o caminho da imagem
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    // salva original e miniatura

                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }

            return View();
        }


        [HttpPost]
        public void DeletarImagem(int id, string nomeimagem)
        {
            string caminhocompleto1 = Request.MapPath("~Images/Uploads/Produto/" + id.ToString() + "/Gallery/" + nomeimagem);
            string caminhocompleto2 = Request.MapPath("~Images/Uploads/Produto/" + id.ToString() + "/Gallery/Thumbs" + nomeimagem);


            if (System.IO.File.Exists(caminhocompleto1))
                System.IO.File.Delete(caminhocompleto1);


            if (System.IO.File.Exists(caminhocompleto2))
                System.IO.File.Delete(caminhocompleto2);
        }


        public ActionResult Excluir(int id)
        {
            // Deleta produto de DB
            using (Db db = new Db())
            {
                Produto produto = db.Produto.Find(id);
                db.Produto.Remove(produto);

                db.SaveChanges();
            }

            // Deleta produto da pasta
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(), "Produto\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            // Redireciona
            return RedirectToAction("Index");
        }

        public ActionResult Detalhes(int id)
        {

            ProdutoVM model;
            using (Db db = new Db())
            {
                Produto produto = db.Produto.Find(id);

                model = new ProdutoVM
                {
                    Produto = produto
                };

            }

            // Get galeria de images
            model.Galeria = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Produto/" + id + "/Gallery/Thumbs"))
                                                .Select(fn => Path.GetFileName(fn));

            // Return view com modelo
            return View("Detalhes", model);
        }




    }
}