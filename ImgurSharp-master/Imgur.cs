﻿using System;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Web;
using System.Net;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
namespace ImgurSharp
{
    public class Imgur
    {
        #region Properties
        public const string UrlToken = "https://api.imgur.com/oauth2/token";
        public const string UrlAuth = "https://api.imgur.com/oauth2/authorize";
        public const string UrlSignin = "https://imgur.com/signin";
        public const string UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        public const string BaseUrl = "https://api.imgur.com/3/";


        public JavaScriptSerializer serializer;
        public string clientID;
        public string clientSecret;
        public string user;
        public string password;
        public CookieContainer cookies;
        #endregion


        #region Constructor
        /// <summary>
        /// Constructor of Imgur object
        /// </summary>
        /// <param name="clientID">Id of application, so Imgur knows which app is submitting data</param>
        public Imgur(string clientID)
        {
            this.clientID = clientID;
        }
        /// <summary>
        /// Constructor of Imgur object with Username and Password;
        /// </summary>
        /// <param name="user">Username account</param>
        /// <param name="password">Password account</param>
        /// <param name="clientID">Client ID</param>
        /// <param name="clientSecret">Client Secret</param>
        public Imgur(string user, string password, string clientID, string clientSecret)
        {
            this.password = password;
            this.user = user;
            this.clientID = clientID;
            this.clientSecret = clientSecret;
            this.cookies = new CookieContainer();
            this.serializer = new JavaScriptSerializer();
        }
        #endregion


        #region ApiMethods
        /// <summary>
        /// Create Session
        /// </summary>
        /// <returns></returns>
        public void CreateSession()
        {
            Console.WriteLine("Creating session.");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlSignin);
            HttpWebResponse response;

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

            request.UserAgent = UserAgent;
            request.AllowAutoRedirect = true;
            request.KeepAlive = true;
            request.Referer = UrlSignin;
            request.CookieContainer = cookies;
            RequestParameters postData = new RequestParameters();
            postData["username"] = user;
            postData["password"] = password;
            postData["remember"] = "remember";
            postData.Add("submit");
            postData.AddToRequest(request);

            response = (HttpWebResponse)request.GetResponse();
            cookies.Add(response.Cookies); ;
            Console.WriteLine("Created session");
        }
        /// <summary>
        /// Request Pin
        /// </summary>
        /// <returns>string Pin</returns>
        public string RequestPin()
        {
            Console.WriteLine("Requesting pin");
            string pin = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlAuth + "?client_id=" + clientID + "&response_type=pin");
            HttpWebResponse response;

            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "ext/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

            request.UserAgent = UserAgent;
            request.AllowAutoRedirect = true;
            request.CookieContainer = cookies;

            response = (HttpWebResponse)request.GetResponse();
            cookies.Add(response.Cookies);
            pin = response.ResponseUri.Query;
            if (!pin.Contains("?pin="))
                throw (new Exception("Response does not contain pin: " + response));
            pin = pin.Substring(pin.IndexOf("?pin=") + 5);
            if (pin.Contains("&"))
                pin = pin.Substring(0, pin.IndexOf("&"));
            //Console.WriteLine("Recieved pin: " + pin);
            return pin;
        }
        /// <summary>
        /// Request Pin
        /// </summary>
        /// <param name="pin">Pin (from RequestPin)</param>
        /// <returns>ImgurToken object</returns>
        public async Task<ImgurToken> RequestToken(string pin)
        {
            Console.WriteLine("Requesting token");
            //get token
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
                var formContent = new FormUrlEncodedContent(new[] { 
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "pin"),
                    new KeyValuePair<string, string>("pin", pin) });

                HttpResponseMessage response = await client.PostAsync(new Uri(UrlToken), formContent);
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                System.Console.WriteLine(content);
                ImgurToken deleteRoot = JsonConvert.DeserializeObject<ImgurToken>(content);
                return deleteRoot;
            }
        }
        /// <summary>
        /// Upload Image
        /// </summary>
        /// <param name="imageStream">Stream of image</param>
        /// <param name="name">Name of image</param>
        /// <param name="title">Title of image</param>
        /// <param name="description">Description of image</param>
        /// <returns>ImgurImage object</returns>
        public async Task<ImgurImage> UploadImage(Stream imageStream, string title, string description, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);
                string base64Image = PhotoStreamToBase64(imageStream);

                var formContent = new FormUrlEncodedContent(new[] { 
                    new KeyValuePair<string, string>("image", base64Image),
                    new KeyValuePair<string, string>("type", "base64"),
                    new KeyValuePair<string, string>("name", "Anonymous"),
                    new KeyValuePair<string, string>("title", title),
                    new KeyValuePair<string, string>("description", description)
                });
                HttpResponseMessage response = await client.PostAsync(new Uri(BaseUrl + "upload"), formContent);
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                ImgurRootObject<ImgurImage> imgRoot = JsonConvert.DeserializeObject<ImgurRootObject<ImgurImage>>(content);

                return imgRoot.Data;
            }
        }
        /// <summary>
        /// Upload Image
        /// </summary>
        /// <param name="url">Url to image http://some.url.to.image.com/image.jpg</param>
        /// <param name="name">Name of image</param>
        /// <param name="title">Title of image</param>
        /// <param name="description">Description of image</param>
        /// <param name="token">Token account</param>
        /// <returns>ImgurImage object</returns>
        public async Task<ImgurImage> UploadImage(string url, string title, string description, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);
                var formContent = new FormUrlEncodedContent(new[] { 
                    new KeyValuePair<string, string>("image", url),
                    new KeyValuePair<string, string>("name", "Anonymous"),
                    new KeyValuePair<string, string>("title", title),
                    new KeyValuePair<string, string>("description", description) });

                HttpResponseMessage response = await client.PostAsync(new Uri(BaseUrl + "upload"), formContent);
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();


                ImgurRootObject<ImgurImage> imgRoot = JsonConvert.DeserializeObject<ImgurRootObject<ImgurImage>>(content);

                return imgRoot.Data;
            }
        }
        /// <summary>
        /// Deletes Image from Imgur
        /// </summary>
        /// <param name="key">DeleteHash or Id of Image, attained when creating image</param>
        /// <param name="token">Token account</param>
        /// <returns>bool of result</returns>
        public async Task<bool> DeleteImage(string key, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);
                HttpResponseMessage response = await client.DeleteAsync(new Uri(BaseUrl + "image/" + key));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                ImgurRootObject<bool> deleteRoot = JsonConvert.DeserializeObject<ImgurRootObject<bool>>(content);

                return deleteRoot.Data;
            }
        }
        /// <summary>
        /// Update Image
        /// </summary>
        /// <param name="deleteHash">DeleteHash  or Id of Image, attained when created</param>
        /// <param name="title">New title</param>
        /// <param name="description">New Description</param>
        /// <returns>bool of result</returns>
        public async Task<bool> UpdateImage(string key, string title, string description, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);

                var formContent = new FormUrlEncodedContent(new[] { 
                    new KeyValuePair<string, string>("description", description),
                    new KeyValuePair<string, string>("title", title)
                 });

                HttpResponseMessage response = await client.PutAsync(new Uri(BaseUrl + "image/" + key), formContent);
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                ImgurRootObject<bool> deleteRoot = JsonConvert.DeserializeObject<ImgurRootObject<bool>>(content);

                return deleteRoot.Data;

            }
        }
        /// <summary>
        /// Creates an Album on Imgur
        /// </summary>
        /// <param name="imageIds">List of string, ImageIds</param>
        /// <param name="title">Title of album</param>
        /// <param name="description">Description of album</param>
        /// <param name="privacy">Privacy level, use NONE for standard</param>
        /// <param name="layout">Layout, use NONE for standard</param>
        /// <param name="coverImageId">Cover image of album, imageId. Should be in the album</param>
        /// <param name="token">Token account</param>
        /// <returns>ImgurCreateAlbum which contains deletehash and link</returns>
        public async Task<ImgurCreateAlbum> CreateAlbum(IEnumerable<string> imageIds, string title, string description, ImgurAlbumPrivacy privacy, ImgurAlbumLayout layout, string coverImageId, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);

                var formContent = new FormUrlEncodedContent(new[] { 
                    new KeyValuePair<string, string>("ids", imageIds.Aggregate((a,b) => a + "," + b)),
                    new KeyValuePair<string, string>("title", title),
                    new KeyValuePair<string, string>("description", description),
                    new KeyValuePair<string, string>("privacy", GetNameFromEnum<ImgurAlbumPrivacy>((int)privacy)),
                    new KeyValuePair<string, string>("layout", GetNameFromEnum<ImgurAlbumLayout>((int)layout)),
                    new KeyValuePair<string, string>("cover", coverImageId),
                 });

                HttpResponseMessage response = await client.PostAsync(new Uri(BaseUrl + "album"), formContent);
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();

                ImgurRootObject<ImgurCreateAlbum> createRoot = JsonConvert.DeserializeObject<ImgurRootObject<ImgurCreateAlbum>>(content);

                return createRoot.Data;
            }
        }
        /// <summary>
        /// Updates ImgurAlbum
        /// </summary>
        /// <param name="key">DeleteHash or Id of Image, obtained at creation</param>
        /// <param name="imageIds">List images in the album</param>
        /// <param name="title">New title</param>
        /// <param name="description">New description</param>
        /// <param name="privacy">New privacy level, use NONE for standard</param>
        /// <param name="layout">New layout, use NONE for standard</param>
        /// <param name="cover">new coverImage, imageId</param>
        /// <param name="token">Token account</param>
        /// <returns>bool of result</returns>
        public async Task<bool> UpdateAlbum(string key, IEnumerable<string> imageIds, string title, string description, ImgurAlbumPrivacy privacy, ImgurAlbumLayout layout, string cover, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);

                var formContent = new FormUrlEncodedContent(new[] { 
                    new KeyValuePair<string, string>("ids", imageIds.Aggregate((a,b) => a + "," + b)),
                    new KeyValuePair<string, string>("title", title),
                    new KeyValuePair<string, string>("description", description),
                    new KeyValuePair<string, string>("privacy", GetNameFromEnum<ImgurAlbumPrivacy>((int)privacy)),
                    new KeyValuePair<string, string>("layout", GetNameFromEnum<ImgurAlbumLayout>((int)layout)),
                    new KeyValuePair<string, string>("cover", cover),
                 });

                HttpResponseMessage response = await client.PutAsync(new Uri(BaseUrl + "album/" + key), formContent);
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();

                ImgurRootObject<bool> updateRoot = JsonConvert.DeserializeObject<ImgurRootObject<bool>>(content);

                return updateRoot.Data;
            }
        }
        /// <summary>
        /// Deletes album from Imgur
        /// </summary>
        /// <param name="key">DeleteHash or Id of Image, obtained when creating Album</param>
        /// <param name="token">Token account</param>
        /// <returns></returns>
        public async Task<bool> DeleteAlbum(string key, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);

                HttpResponseMessage response = await client.DeleteAsync(new Uri(BaseUrl + "album/" + key));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                ImgurRootObject<bool> deleteRoot = JsonConvert.DeserializeObject<ImgurRootObject<bool>>(content);

                return deleteRoot.Data;
            }
        }
        /// <summary>
        /// Add images to excisting album
        /// </summary>
        /// <param name="key">DeleteHash or Id of Image, obtained when creating album</param>
        /// <param name="imageIds">ALL images must be here, imgur will otherwise remove the ones missing</param>
        /// <param name="token">Token account</param>
        /// <returns></returns>
        public async Task<bool> AddImagesToAlbum(string key, IEnumerable<string> imageIds, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);

                var formContent = new FormUrlEncodedContent(new[] { 
                    new KeyValuePair<string, string>("ids", imageIds.Aggregate((a,b) => a + "," + b))
                });

                HttpResponseMessage response = await client.PostAsync(new Uri(BaseUrl + "album/" + key), formContent);

                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();

                ImgurRootObject<bool> addRoot = JsonConvert.DeserializeObject<ImgurRootObject<bool>>(content);

                return addRoot.Data;
            }
        }
        /// <summary>
        /// Removes images from album
        /// </summary>
        /// <param name="key">DeleteHash or Id of Image, obtained when creating album</param>
        /// <param name="imageIds">ALL images must be here, imgur will otherwise remove the ones missing</param>
        /// <param name="token">Token account</param>
        /// <returns></returns>
        public async Task<bool> RemoveImagesFromAlbum(string key, IEnumerable<string> imageIds, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);

                HttpResponseMessage response = await client.DeleteAsync(new Uri(BaseUrl + "album/" + key + "/remove_images?ids=" + imageIds.Aggregate((a, b) => a + "," + b)));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                ImgurRootObject<bool> removeRoot = JsonConvert.DeserializeObject<ImgurRootObject<bool>>(content);

                return removeRoot.Data;
            }
        }
        /// <summary>
        /// Gets an album from Imgur
        /// </summary>
        /// <param name="albumId">Id of Album</param>
        /// <returns></returns>
        public async Task<ImgurAlbum> GetAlbum(string albumId)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client,"");
                HttpResponseMessage response = await client.GetAsync(new Uri(BaseUrl + "album/" + albumId));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
                ImgurRootObject<ImgurAlbum> albumRoot = JsonConvert.DeserializeObject<ImgurRootObject<ImgurAlbum>>(content);

                return albumRoot.Data;
            }
        }
        /// <summary>
        /// Gets an image from Imgur
        /// </summary>
        /// <param name="imageId">Id of Image</param>
        /// <returns></returns>
        public async Task<ImgurImage> GetImage(string imageId)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client,"");
                HttpResponseMessage response = await client.GetAsync(new Uri(BaseUrl + "image/" + imageId));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();

                ImgurRootObject<ImgurImage> imageRoot = JsonConvert.DeserializeObject<ImgurRootObject<ImgurImage>>(content);

                return imageRoot.Data;
            }
        }
        /// <summary>
        /// Get Account
        /// </summary>
        /// <param name="username">Username Account</param>
        /// <returns>ImgurAccount object</returns>
        public async Task<ImgurAccount> GetAccount(string username)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, "");
                //var formContent = new FormUrlEncodedContent(username);
                HttpResponseMessage response = await client.GetAsync(new Uri(BaseUrl + "account/" + username));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();

                ImgurRootObject<ImgurAccount> imgRoot = JsonConvert.DeserializeObject<ImgurRootObject<ImgurAccount>>(content);

                return imgRoot.Data;
            }
        }
        /// <summary>
        /// Get Account
        /// </summary>
        /// <param name="username">Username Account</param>
        /// <param name="token">Token Account</param>
        /// <returns>Amount Image</returns>
        public async Task<long> GetImageCount(string username, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);
                //var formContent = new FormUrlEncodedContent(username);
                HttpResponseMessage response = await client.GetAsync(new Uri(BaseUrl + "account/" + username + "/images/count"));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();

                ImgurRootObject<long> imgRoot = JsonConvert.DeserializeObject<ImgurRootObject<long>>(content);
                return imgRoot.Data;
            }
        }
        /// <summary>
        /// Get list ID image from account
        /// </summary>
        /// <param name="username">Username Account</param>
        /// <param name="token">Token Account</param>
        /// <returns>List String</returns>
        public async Task<List<string>> GetImageIDs(string username, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);
                //var formContent = new FormUrlEncodedContent(username);
                HttpResponseMessage response = await client.GetAsync(new Uri(BaseUrl + "account/" + username + "/images/ids/"));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
                ImgurRootObject<List<string>> imgRoot = JsonConvert.DeserializeObject<ImgurRootObject<List<string>>>(content);
                return imgRoot.Data;
            }
        }
        /// <summary>
        /// Get list ImgurImage from account
        /// </summary>
        /// <param name="username">Username Account</param>
        /// <param name="token">Token Account</param>
        /// <returns>List ImgurImage</returns>
        public async Task<List<ImgurImage>> GetImages(string username, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                SetHeader(client, token);
                //var formContent = new FormUrlEncodedContent(username);
                HttpResponseMessage response = await client.GetAsync(new Uri(BaseUrl + "account/" + username + "/images/"));
                await CheckHttpStatusCode(response);
                string content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
                ImgurRootObject<List<ImgurImage>> imgRoot = JsonConvert.DeserializeObject<ImgurRootObject<List<ImgurImage>>>(content);
                return imgRoot.Data;
            }
        }
        #endregion


        #region Helpers
        string PhotoStreamToBase64(Stream stream)
        {
            MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            byte[] result = memoryStream.ToArray();

            string base64img = System.Convert.ToBase64String(result);
            return base64img;
        }
        private async Task CheckHttpStatusCode(HttpResponseMessage responseMessage)
        {
            //Imgur StatusCodes
            var content = await responseMessage.Content.ReadAsStringAsync();
            ImgurRootObject<ImgurRequestError> errorRoot = null;

            try
            {
                errorRoot = JsonConvert.DeserializeObject<ImgurRootObject<ImgurRequestError>>(content);
            }
            catch (Exception)
            {

            }

            if (errorRoot == null)
                return;

            switch ((int)responseMessage.StatusCode)
            {
                case 400:
                case 401:
                case 403:
                case 404:
                case 429:
                case 500:
                    throw new Exception(string.Format(" Error: {0} \n Request: {1} \n Verb: {2} ", errorRoot.Data.Error, errorRoot.Data.Request, errorRoot.Data.Method));
                case 200:
                default:
                    return;

            }
        }
        void SetHeader(HttpClient client, string token)
        {
            if (token != "")
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            }
            else
            {
                client.DefaultRequestHeaders.Add("Authorization", "Client-ID " + clientID);
            }
        }
        string GetNameFromEnum<T>(int selected) where T : struct
        {
            string value = Enum.GetName(typeof(T), selected).ToLower();

            if (value == "none")
                value = "";

            return value;
        }
        #endregion
    }
}