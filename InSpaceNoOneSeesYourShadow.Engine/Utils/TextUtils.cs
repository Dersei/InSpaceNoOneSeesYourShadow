﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InSpaceNoOneSeesYourShadow.Engine.Utils
{
    internal class TextUtils
    {
        private const string Characters = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789µ§½!""#¤%&/()=?^*@£€${[]}\~¨'-_.:,;<>|°©®±¥";

        public Bitmap GenerateCharacters(int fontSize, string fontName, out Size charSize)
        {
            var characters = new List<Bitmap>();
            using var font = new Font(fontName, fontSize);
            foreach (var ch in Characters)
            {
                var charBmp = GenerateCharacter(font, ch);
                characters.Add(charBmp);
            }
            charSize = new Size(characters.Max(x => x.Width), characters.Max(x => x.Height));
            var charMap = new Bitmap(charSize.Width * characters.Count, charSize.Height);
            using (var gfx = Graphics.FromImage(charMap))
            {
                gfx.FillRectangle(Brushes.Black, 0, 0, charMap.Width, charMap.Height);
                for (int i = 0; i < characters.Count; i++)
                {
                    var c = characters[i];
                    gfx.DrawImageUnscaled(c, i * charSize.Width, 0);

                    c.Dispose();
                }
            }
            return charMap;
        }

        private Bitmap GenerateCharacter(Font font, char c)
        {
            var size = GetSize(font, c);
            var bmp = new Bitmap((int)size.Width, (int)size.Height);
            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);
                gfx.DrawString(c.ToString(), font, Brushes.White, 0, 0);
            }
            return bmp;
        }
        private SizeF GetSize(Font font, char c)
        {
            using var bmp = new Bitmap(512, 512);
            using var gfx = Graphics.FromImage(bmp);
            return gfx.MeasureString(c.ToString(), font);
        }
    }
}
