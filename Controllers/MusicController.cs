using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using System.Web.Mvc;

namespace SensitiveBot.Controllers
{
    public class MusicController : Controller
    {
        public MusicController()
        {
        }

        [Route("Music/MusicPlayerFrame/{sid?}")]
        public ActionResult MusicPlayerFrame(int sid)
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.

            ViewData["SongID"] = sid;

            return View();
        }

        [Route("Music/MusicPlayer/{sid?}")]
        public ActionResult MusicPlayer(int sid)
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.

            string dirPath = "wwwroot\\MusicLibs";
            List<string> dirs = new List<string>(Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories));

            int n1 = dirs.Count();

            sid = sid < 0 ? 0 : sid;
            sid = sid >= n1 ? n1 - 1 : sid;

            string path = "" + dirs[sid];
            List<string> files = new List<string>(Directory.GetFiles(path, "*.mp3"));

            string AlbumName = Path.GetFileName(path);

            int n2 = files.Count();
            for (int i = 0; i < n2; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

            ViewData["SongID"] = sid;
            ViewData["AlbumName"] = AlbumName;
            ViewData["SongList"] = string.Join("|", files);

            return View();
        }
    }
}
