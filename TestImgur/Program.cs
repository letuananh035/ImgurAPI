using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImgurSharp;
using System.IO;
namespace TestImgur
{
    class Program
    {
        static Imgur API;
        static void Main(string[] args)
        {
            API = new Imgur("letuananh035", "bate7a1954753", "720e8b4bd4a8468", "eebc8cc67b44d1341666373e014a4b22ca84694b");
            UploadImage();
            System.Console.ReadLine();

        }
        private static async void UploadImage()
        {
            try
            {
                API.CreateSession();
                bool Check = await API.ChechToken(" ");
                System.Console.WriteLine(Check);
                string Code = API.RequestPin();
                ImgurToken Token = await API.RequestTokenWithPin(Code);
                System.Console.WriteLine(Token.Access_token);
                
                //Image
                ImgurAccount Account = await API.GetAccount("letuananh035");
                System.Console.WriteLine(Account.Id);
                System.Console.WriteLine(Account.Description);
                System.Console.WriteLine(Account.URL);
                List<ImgurImage> ImageCount = await API.GetImages(Token.Access_token);
                System.Console.WriteLine(ImageCount[0].Link);
              
                ImgurImage urlImage = await API.UploadImage("https://assets-cdn.github.com/images/modules/logos_page/GitHub-Mark.png", "title", "description","");
                System.Console.Write(urlImage.Link);
                byte[] buff = File.ReadAllBytes("vs-icon.png");
                MemoryStream ms = new MemoryStream(buff);

                ImgurImage streamImage = await API.UploadImage(ms, "title", "description", Token.Access_token);
                
                bool updated = await API.UpdateImage(streamImage.Id, "updated title", "a new description", Token.Access_token);

                ImgurImage getImage = await API.GetImage(streamImage.Id);
                System.Console.Write(getImage.Link);
                //Album
                ImgurCreateAlbum createdAlbum = await API.CreateAlbum(new string[] { streamImage.Id }, "album title", "album description", ImgurAlbumPrivacy.Public, ImgurAlbumLayout.Horizontal, streamImage.Id, Token.Access_token);

                bool result = await API.UpdateAlbum(createdAlbum.Id, new string[] { streamImage.Id, urlImage.Id }, "updated album title", "update album description", ImgurAlbumPrivacy.Hidden, ImgurAlbumLayout.Blog, urlImage.Id, Token.Access_token);
                bool addImagesResult = await API.AddImagesToAlbum(createdAlbum.DeleteHash, new string[] { streamImage.Id, urlImage.Id }, Token.Access_token);
                ImgurAlbum album = await API.GetAlbum(createdAlbum.Id);
                bool removeImagesResult = await API.RemoveImagesFromAlbum(createdAlbum.DeleteHash, new string[] { urlImage.Id }, Token.Access_token);
               


                //Delete
                bool deleteAlbum = await API.DeleteAlbum(createdAlbum.Id, Token.Access_token);
                bool deletedUrlImage = await API.DeleteImage(urlImage.Deletehash, "");
                bool deletedStreamImage = await API.DeleteImage(streamImage.Id, Token.Access_token);
  
            }
            catch (Exception e)
            {
                System.Console.Write(e.Message);
            }
        }
    }
}
