////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Platform.Test;
using Controls.Properties;
using System.Threading;

namespace Microsoft.SPOT.Platform.Tests
{
    public class TextTests : Master_Controls, IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for Text tests.");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");
        }

        [TestMethod]
        public MFTestResults Text_DefaultConstructorTest1()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _default = true;
            _txtContent = "Default Constructor Test";
            _font = Resources.GetFont(Resources.FontResources.NinaB);
            UpdateWindow();
            if (!(CheckContentAndFont(_getText, _txtContent, _getFont, _font)))
            {
                Log.Comment("Failure verifying Default Constructor");
                testResult = MFTestResults.Fail;
            }
            _default = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_SingleArgConstructorTest2()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _singleArg = true;
            _txtContent = "Single Argument Constructor Test";
            _font = Resources.GetFont(Resources.FontResources.small);
            UpdateWindow();
            if (!(CheckContentAndFont(_getText, _txtContent, _getFont, _font)))
            {
                Log.Comment("Failure verifying Single Arguemnt Constructor");
                testResult = MFTestResults.Fail;
            }
            _singleArg = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_TowArgConstructorTest3()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            _default = false;
            _singleArg = false;
            _txtContent = "Two Argument Constructor Test";
            _font = Resources.GetFont(Resources.FontResources.NinaB);
            UpdateWindow();
            if (!(CheckContentAndFont(_getText, _txtContent, _getFont, _font)))
            {
                Log.Comment("Failure verifying Two Arguemnt Constructor");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_FontTest4()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Settig the Font and verifying by getting the Font");
            Font _nFont = Resources.GetFont(Resources.FontResources.NinaB);
            Font _sFont = Resources.GetFont(Resources.FontResources.small);
            _font = _nFont;
            _txtContent = "Text.Font Test !";
            UpdateWindow();
            if (_getFont != _nFont)
            {
                Log.Comment("Failure : _text.Font didn't return NinaB Font");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Setting different Font and verifying");
            _font = _sFont;
            UpdateWindow();
            if (_getFont != _sFont)
            {
                Log.Comment("Failure : _text.Font didn't return samll Font");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_ForeColorTest5()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Changing the ForeColor and verifying");
            Color[] _color = new Color[] { Colors.Red, Colors.Green, Colors.Blue };
            _txtContent = "ForeColor Test";
            for (int i = 0; i < _color.Length; i++)
            {
                _foreColor = _color[i];
                UpdateWindow();
                if (_getForeColor != _color[i])
                {
                    Log.Comment("Expected ForeColor '" + _color[i] + "' but got '" + _getForeColor + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_TextContentTest6()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            for (int i = 0; i < strArr.Length; i++)
            {
                _txtContent = strArr[i];
                UpdateWindow();
                if (_getText != strArr[i])
                {
                    Log.Comment("Expected TextContent '" + strArr[i] + "' but got '" + _getText + "'");
                    testResult = MFTestResults.Fail;
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_LineHeightTest7()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            for (int i = 0; i < strArr.Length; i++)
            {
                _txtContent = strArr[i];
                UpdateWindow();
                if (_getLineHeight != (_font.Height + _font.ExternalLeading))
                {
                    Log.Comment("Expected LineHeight = '" + (_font.Height + _font.ExternalLeading) +
                        "' (_font.Height + _font.ExternalLeading) but got '" + _getLineHeight + "'");
                }
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_TextWrapTest8()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Setting and Getting Text.TextWrap and Verifying");
            _txtContent = "If the bee disappeared off the surface of the globe then man " +
            "would only have four years of life left. This sentence should be wrapped by the UI"; ;
            UpdateWindow();
            if (_getWrapText)
            {
                Log.Comment("Text.TextWrap shouldn't return true while the Text is not Wrapped");
                testResult = MFTestResults.Fail;
            }
            _wrapText = true;
            UpdateWindow();
            if (!_getWrapText)
            {
                Log.Comment("Text.TextWrap should return true if Text is Wrapped");
            }
            _wrapText = false;

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_TextAlignmentTest9()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Verifying TextAlignment by default is TextAlignment.Left");
            _txtContent = "Text Under Test";
            UpdateWindow();
            if (_getTextAlignment != TextAlignment.Left)
            {
                Log.Comment("Expected Default TextAlignment to be '" + TextAlignment.Left
                    + "'(TextAlignment.Left) but got '" + _getTextAlignment + "' ");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Setting TextAlignment to Center and verifying");
            _align = true;
            _txtAlignment = TextAlignment.Center;
            UpdateWindow();
            if (_getTextAlignment != TextAlignment.Center)
            {
                Log.Comment("Expected TextAlignment to be '" + TextAlignment.Center
                        + "'(TextAlignment.Center) but got '" + _getTextAlignment + "' ");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Setting TextAlignment to Right and verifying");
            _txtAlignment = TextAlignment.Right;
            UpdateWindow();
            if (_getTextAlignment != TextAlignment.Right)
            {
                Log.Comment("Expected TextAlignment to be '" + TextAlignment.Right
                        + "'(TextAlignment.Right) but got '" + _getTextAlignment + "' ");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Setting TextAlignment to Left and verifying");
            _txtAlignment = TextAlignment.Left;
            UpdateWindow();
            if (_getTextAlignment != TextAlignment.Left)
            {
                Log.Comment("Expected TextAlignment to be '" + TextAlignment.Left
                        + "'(TextAlignment.Left) but got '" + _getTextAlignment + "' ");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        [TestMethod]
        public MFTestResults Text_TextTextTrimmingTest10()
        {
            MFTestResults testResult = MFTestResults.Pass;
            if (CleaningWindow() != MFTestResults.Pass)
            {
                return MFTestResults.Fail;
            }
            Log.Comment("Verifying TextTrimming by default is WordEllipsis");
            _txtContent = "The first rule of any technology used in a business is that" +
                " automation applied to an efficient operation will magnify the efficiency";
            UpdateWindow();
            if (_getTextTrimming != TextTrimming.WordEllipsis)
            {
                Log.Comment("Expected Default TextTrimming to to be '" + TextTrimming.WordEllipsis
                        + "'(TextTrimming.WordEllipsis) but got '" + _getTextTrimming + "' ");
                testResult = MFTestResults.Fail;
            }
            _trimming = true;
            Log.Comment("Setting TextTrimming to CharacterEllipsis and verifying");
            _txtTrimming = TextTrimming.CharacterEllipsis;
            UpdateWindow();
            if (_getTextTrimming != TextTrimming.CharacterEllipsis)
            {
                Log.Comment("Expected TextTrimming to be '" + TextTrimming.CharacterEllipsis
                        + "'(TextTrimming.CharacterEllipsis) but got '" + _getTextTrimming + "' ");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Setting TextTrimming to None and verifying");
            _txtTrimming = TextTrimming.None;
            UpdateWindow();
            if (_getTextTrimming != TextTrimming.None)
            {
                Log.Comment("Expected TextTrimming to be '" + TextTrimming.None
                        + "'(TextTrimming.None) but got '" + _getTextTrimming + "' ");
                testResult = MFTestResults.Fail;
            }
            Log.Comment("Setting TextTrimming to WordEllipsis and verifying");
            _txtTrimming = TextTrimming.WordEllipsis;
            UpdateWindow();
            if (_getTextTrimming != TextTrimming.WordEllipsis)
            {
                Log.Comment("Expected TextTrimming to be '" + TextTrimming.WordEllipsis
                        + "'(TextTrimming.WordEllipsis) but got '" + _getTextTrimming + "' ");
                testResult = MFTestResults.Fail;
            }

            return testResult;
        }

        private bool CheckContentAndFont(string str1, string str2, Font f1, Font f2)
        {
            bool bRet = true;
            if (str1 != str2)
            {
                Log.Comment("Expected '" + str2 + "' but got '" + str1 + "'");
                bRet = false;
            }
            if (f1 != f2)
            {
                Log.Comment("Got different Fonts");
                bRet = false;
            }
            return bRet;
        }

        Text _text = null;
        Font _getFont;
        TextAlignment _txtAlignment, _getTextAlignment;
        TextTrimming _txtTrimming, _getTextTrimming;
        Color _foreColor, _getForeColor;
        int _getLineHeight;
        bool _default = false;
        bool _singleArg = false;
        bool _wrapText = false, _getWrapText = false, _align = false, _trimming = false;
        string _txtContent = null, _getText;
        string[] strArr = new string[]{"", "  ", "\b\b\b\b\b\b\b\b\b\b\b\b", 
            @".dfjk' ddfkjk\b\s\r\v]\a\q\e\\e\9393\qakjfe\kdfj\akjfd_933;a023",
            MFUtilities.GetRandomString(70),
            "This should be a very long string by the time that I am done with "+
            " it, perhaps this would be easier to generate if I was \r\n \b\v\t\038"+
            " using a script\0",};

        object Text_UpdateWindow(object obj)
        {
            if (_default)
            {
                _text = new Text();
                _text.Font = _font;
                _text.TextContent = _txtContent;
            }
            else if (_singleArg)
            {
                _text = new Text(_txtContent);
                _text.Font = _font;
            }
            else
            {
                _text = new Text(_font, _txtContent);
            }
            if (_wrapText)
            {
                _text.TextWrap = true;
                _text.Height = 200;
                _text.Width = 100;
            }
            if (_align)
            {
                _text.TextAlignment = _txtAlignment;
            }
            if (_trimming)
            {
                _text.Trimming = _txtTrimming;
            }
            _text.ForeColor = _foreColor;
            _getForeColor = _text.ForeColor;
            _getText = _text.TextContent;
            _getFont = _text.Font;
            _getLineHeight = _text.LineHeight;
            _getWrapText = _text.TextWrap;
            _getTextAlignment = _text.TextAlignment;
            _getTextTrimming = _text.Trimming;
            mainWindow.Child = _text;

            return null;
        }

        void UpdateWindow()
        {
            Master_Controls.mainWindow.Dispatcher.Invoke(new TimeSpan(0, 0, 5), new DispatcherOperationCallback(Text_UpdateWindow), null);

            _autoEvent.WaitOne();
        }
    }
}
