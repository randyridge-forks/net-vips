﻿using System;
using System.Linq;
using Xunit;

namespace NetVips.Tests
{
    public class ColourTests : IClassFixture<TestsFixture>
    {
        [Fact]
        public void TestColourspace()
        {
            // mid-grey in Lab ... put 42 in the extra band, it should be copied
            // unmodified
            var test = Image.Black(100, 100) + new[] {50, 0, 0, 42};
            test = test.Copy(interpretation: Enums.Interpretation.Lab);

            // a long series should come in a circle
            var im = test;
            foreach (var col in Helper.ColourColourspaces.Concat(new[] {Enums.Interpretation.Lab}))
            {
                im = im.Colourspace(col);
                Assert.Equal(col, im.Interpretation);

                for (var i = 0; i < 4; i++)
                {
                    var minL = im[i].Min();
                    var maxH = im[i].Max();
                    Assert.Equal(minL, maxH);
                }

                var pixel = im.Getpoint(10, 10);
                Assert.Equal(42, pixel[3], 2);
            }

            // alpha won't be equal for RGB16, but it should be preserved if we go
            // there and back
            im = im.Colourspace(Enums.Interpretation.Rgb16);
            im = im.Colourspace(Enums.Interpretation.Lab);

            var before = test.Getpoint(10, 10);
            var after = im.Getpoint(10, 10);
            Helper.AssertAlmostEqualObjects(before, after, 0.1);

            // go between every pair of colour spaces
            foreach (var start in Helper.ColourColourspaces)
            {
                foreach (var end in Helper.ColourColourspaces)
                {
                    im = test.Colourspace(start);
                    var im2 = im.Colourspace(end);
                    var im3 = im2.Colourspace(Enums.Interpretation.Lab);
                    before = test.Getpoint(10, 10);
                    after = im3.Getpoint(10, 10);
                    Helper.AssertAlmostEqualObjects(before, after, 0.1);
                }
            }

            // test Lab->XYZ on mid-grey
            // checked against http://www.brucelindbloom.com
            im = test.Colourspace(Enums.Interpretation.Xyz);
            after = im.Getpoint(10, 10);
            Helper.AssertAlmostEqualObjects(new[]
            {
                17.5064,
                18.4187,
                20.0547,
                42
            }, after);

            // grey->colour->grey should be equal
            foreach (var monoFmt in Helper.MonoColourspaces)
            {
                var testGrey = test.Colourspace(monoFmt);
                im = testGrey;
                foreach (var col in Helper.ColourColourspaces.Concat(new[] {monoFmt}))
                {
                    im = im.Colourspace(col);
                    Assert.Equal(col, im.Interpretation);
                }

                var pixelBefore = testGrey.Getpoint(10, 10);
                var alphaBefore = pixelBefore[1];
                var pixelAfter = im.Getpoint(10, 10);
                var alphaAfter = pixelAfter[1];
                Assert.True(Math.Abs(alphaAfter - alphaBefore) < 1);

                // GREY16 can wind up rather different due to rounding but 8-bit we should hit exactly
                Assert.True(
                    Math.Abs(pixelAfter[0] - pixelBefore[0]) < (monoFmt == Enums.Interpretation.Grey16 ? 30 : 1));
            }
        }

        /// <summary>
        /// test results from Bruce Lindbloom's calculator:s
        /// http://www.brucelindbloom.com
        /// </summary>
        [Fact]
        public void TestDE00()
        {
            // put 42 in the extra band, it should be copied unmodified
            var reference = Image.Black(100, 100) + new[] {50, 10, 20, 42};
            reference = reference.Copy(interpretation: Enums.Interpretation.Lab);
            var sample = Image.Black(100, 100) + new[] {40, -20, 10};
            sample = sample.Copy(interpretation: Enums.Interpretation.Lab);

            var difference = reference.DE00(sample);
            var diffPixel = difference.Getpoint(10, 10);
            Assert.Equal(30.238, diffPixel[0], 3);
            Assert.Equal(42.0, diffPixel[1], 3);
        }

        [Fact]
        public void TestDE76()
        {
            // put 42 in the extra band, it should be copied unmodified
            var reference = Image.Black(100, 100) + new[] {50, 10, 20, 42};
            reference = reference.Copy(interpretation: Enums.Interpretation.Lab);
            var sample = Image.Black(100, 100) + new[] {40, -20, 10};
            sample = sample.Copy(interpretation: Enums.Interpretation.Lab);

            var difference = reference.DE76(sample);
            var diffPixel = difference.Getpoint(10, 10);
            Assert.Equal(33.166, diffPixel[0], 3);
            Assert.Equal(42.0, diffPixel[1], 3);
        }

        /// <summary>
        /// the vips CMC calculation is based on distance in a colorspace
        /// derived from the CMC formula, so it won't match exactly ...
        /// see vips_LCh2CMC() for details
        /// </summary>
        [Fact]
        public void TestDECMC()
        {
            // put 42 in the extra band, it should be copied unmodified
            var reference = Image.Black(100, 100) + new[] {50, 10, 20, 42};
            reference = reference.Copy(interpretation: Enums.Interpretation.Lab);
            var sample = Image.Black(100, 100) + new[] {55, 11, 23};
            sample = sample.Copy(interpretation: Enums.Interpretation.Lab);

            var difference = reference.DECMC(sample);
            var diffPixel = difference.Getpoint(10, 10);
            Assert.True(Math.Abs(diffPixel[0] - 4.97) < 0.5);
            Assert.Equal(42.0, diffPixel[1], 3);
        }

        [Fact]
        public void TestIcc()
        {
            var test = Image.NewFromFile(Helper.JpegFile);

            var im = test.IccImport().IccExport();
            Assert.True(im.DE76(test).Max() < 6);

            im = test.IccImport();
            var im2 = im.IccExport(depth: 16);
            Assert.Equal(Enums.BandFormat.Ushort, im2.Format);
            var im3 = im2.IccImport();
            Assert.True((im - im3).Abs().Max() < 3);

            im = test.IccImport(intent: Enums.Intent.Absolute);

            im2 = im.IccExport(intent: Enums.Intent.Absolute);
            Assert.True(im2.DE76(test).Max() < 6);

            im = test.IccImport();
            im2 = im.IccExport(outputProfile: Helper.SrgbFile);
            im3 = im.Colourspace(Enums.Interpretation.Srgb);
            Assert.True(im2.DE76(im3).Max() < 6);

            var beforeProfile = test.Get("icc-profile-data") as byte[];
            im = test.IccTransform(Helper.SrgbFile);
            var afterProfile = im.Get("icc-profile-data") as byte[];
            im2 = test.IccImport();
            im3 = im2.Colourspace(Enums.Interpretation.Srgb);
            Assert.True(im2.DE76(im3).Max() < 6);
            Assert.NotEqual(beforeProfile.Length, afterProfile.Length);

            im = test.IccImport(inputProfile: Helper.SrgbFile);
            im2 = test.IccImport();
            Assert.True(6 < im.DE76(im2).Max());

            im = test.IccImport(pcs: Enums.PCS.Xyz);
            Assert.Equal(Enums.Interpretation.Xyz, im.Interpretation);
            im = test.IccImport();
            Assert.Equal(Enums.Interpretation.Lab, im.Interpretation);
        }
    }
}