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
            API = new Imgur("letuananh035", "", "76e738a94a1792c", "2fd2eeea9366ef4ec932f65ce6a64c7cc09780b9");
            UploadImage();
            System.Console.ReadLine();

        }
        private static async void UploadImage()
        {
            try
            {
                API.CreateSession();
                string PIN = API.RequestPin();
                ImgurToken Token = await API.RequestToken(PIN);
                System.Console.WriteLine(Token.Access_token);
                //Image
                List<ImgurImage> ImageCount = await API.GetImages("letuananh035", Token.Access_token);
                System.Console.WriteLine(ImageCount[0].Link);
                ImgurAccount Account = await API.GetAccount("letuananh035");
                System.Console.WriteLine(Account.Id);
                System.Console.WriteLine(Account.Description);
                System.Console.WriteLine(Account.URL);
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
