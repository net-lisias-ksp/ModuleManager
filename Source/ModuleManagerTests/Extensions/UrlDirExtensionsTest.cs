﻿/*
	This file is part of Module Manager /L
		© 2018-2023 LisiasT
		© 2013-2018 Sarbian; Blowfish
		© 2013 ialdabaoth

	Module Manager /L is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Module Manager /L is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 3.0
	along with Module Manager /L. If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using Xunit;
using TestUtils;
using ModuleManager.Extensions;

namespace ModuleManagerTests.Extensions
{
    public class UrlDirExtensionsTest
    {

        [Fact]
        public void TestFind__IndirectChild()
        {
            UrlDir urlDir = UrlBuilder.CreateDir("abc");
            UrlDir.UrlFile urlFile = UrlBuilder.CreateFile("def/ghi.cfg", urlDir);

            Assert.Equal(urlFile, urlDir.Find("def/ghi"));
        }

        [Fact]
        public void TestFind__DirectChild()
        {
            UrlDir urlDir = UrlBuilder.CreateDir("abc");
            UrlDir.UrlFile urlFile = UrlBuilder.CreateFile("def.cfg", urlDir);

            Assert.Equal(urlFile, urlDir.Find("def"));
        }

        [Fact]
        public void TestFind__Extension()
        {
            UrlDir urlDir = UrlBuilder.CreateDir("abc");
            UrlBuilder.CreateFile("def/ghi.yyy", urlDir);
            UrlDir.UrlFile urlFile = UrlBuilder.CreateFile("def/ghi.cfg", urlDir);
            UrlBuilder.CreateFile("def/ghi.zzz", urlDir);

            Assert.Equal(urlFile, urlDir.Find("def/ghi.cfg"));
        }

        [Fact]
        public void TestFind__NotFound()
        {
            UrlDir urlDir = UrlBuilder.CreateDir("abc");
            UrlBuilder.CreateDir("def", urlDir);

            Assert.Null(urlDir.Find("def/ghi"));
        }

        [Fact]
        public void TestFind__Extension__NotFound()
        {
            UrlDir urlDir = UrlBuilder.CreateDir("abc");
            UrlBuilder.CreateFile("def/ghi.yyy", urlDir);
            UrlBuilder.CreateFile("def/ghi.zzz", urlDir);

            Assert.Null(urlDir.Find("def/ghi.cfg"));
        }

        [Fact]
        public void TestFind__IntermediateDirectoryNotFound()
        {
            UrlDir urlDir = UrlBuilder.CreateDir("abc");
            Assert.Null(urlDir.Find("def/ghi"));
        }

        [Fact]
        public void TestFind__UrlDirNull()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                UrlDirExtensions.Find(null, "abc");
            });

            Assert.Equal("urlDir", ex.ParamName);
        }

        [Fact]
        public void TestFind__UrlNull()
        {
            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(delegate
            {
                UrlDirExtensions.Find(UrlBuilder.CreateDir("abc"), null);
            });

            Assert.Equal("url", ex.ParamName);
        }
    }
}
