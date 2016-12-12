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
            API = new Imgur("", "", "", "");
            UploadImage();
            System.Console.ReadLine();

        }
        private static async void UploadImage()
        {
            try
            {
                API.CreateSession();
                //bool Check = await API.ChechToken("");
                //System.Console.WriteLine(Check);
                
                string Code = API.RequestPin();
                if (Code.Contains(".com"))
                {
                    Console.WriteLine(Code);
                    Console.Write("Write Pin: ");
                    Code = Console.ReadLine();
                }
                ImgurToken Token = await API.RequestTokenWithPin(Code);
                System.Console.WriteLine(Token.Access_token);
                ImgurToken ResetToken = await API.ResetToken(Token);
                Token = ResetToken;
                System.Console.WriteLine(ResetToken.Access_token);
                await API.GetImagesAlbum("Chww0");
                long ImageCount = await API.GetImageCount(Token);
                System.Console.WriteLine(ImageCount);

                ImgurAccount Account = await API.GetAccount("letuananh035");
                System.Console.WriteLine(Account.Id);
                System.Console.WriteLine(Account.Description);
                System.Console.WriteLine(Account.URL);
                List<ImgurImage> Images = await API.GetImages(Token);
                System.Console.WriteLine(Images[0].Link);

                ImgurImage urlImage = await API.UploadImage("https://assets-cdn.github.com/images/modules/logos_page/GitHub-Mark.png", "title", "description", Token);
                System.Console.Write(urlImage.Link);
                byte[] buff = File.ReadAllBytes("vs-icon.png");
                MemoryStream ms = new MemoryStream(buff);

                ImgurImage streamImage = await API.UploadImage(ms, "title", "description", Token);
                
                bool updated = await API.UpdateImage(streamImage.Id, "updated title", "a new description", Token);

                ImgurImage getImage = await API.GetImage(streamImage.Id);
                System.Console.Write(getImage.Link);
                //Album
                ImgurCreateAlbum createdAlbum = await API.CreateAlbum(new string[] { streamImage.Id }, "album title", "album description", ImgurAlbumPrivacy.Public, ImgurAlbumLayout.Horizontal, streamImage.Id, Token);

                bool result = await API.UpdateAlbum(createdAlbum.Id, new string[] { streamImage.Id, urlImage.Id }, "updated album title", "update album description", ImgurAlbumPrivacy.Hidden, ImgurAlbumLayout.Blog, urlImage.Id, Token);
                bool addImagesResult = await API.AddImagesToAlbum(createdAlbum.DeleteHash, new string[] { streamImage.Id, urlImage.Id }, Token);
                ImgurAlbum album = await API.GetAlbum(createdAlbum.Id);
                bool removeImagesResult = await API.RemoveImagesFromAlbum(createdAlbum.DeleteHash, new string[] { urlImage.Id }, Token);
               


                //Delete
                bool deleteAlbum = await API.DeleteAlbum(createdAlbum.Id, Token);
                bool deletedUrlImage = await API.DeleteImage(urlImage.Deletehash);
                bool deletedStreamImage = await API.DeleteImage(streamImage.Id, Token);
            
            }
            catch (Exception e)
            {
                System.Console.Write(e.Message);
            }
        }
    }
}
