﻿
//===============================================================================
//
//  IMPORTANT NOTICE, PLEASE READ CAREFULLY:
//
//  => This code is licensed under the GNU General Public License (GPL v3). A copy of the license is available at:
//        https://www.gnu.org/licenses/gpl.txt
//
//  => As stated in the license text linked above, "The GNU General Public License does not permit incorporating your program into proprietary programs". It also does not permit incorporating this code into non-GPL-licensed code (such as MIT-licensed code) in such a way that results in a non-GPL-licensed work (please refer to the license text for the precise terms).
//
//  => Licenses that permit proprietary use are available at:
//        http://www.cshtml5.com
//
//  => Copyright 2019 Userware/CSHTML5. This code is part of the CSHTML5 product (cshtml5.com).
//
//===============================================================================



using CSHTML5.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Collections;

#if MIGRATION
using System.Windows.Documents;
#else
using Windows.UI.Text;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Documents;
#endif

#if MIGRATION
namespace System.Windows.Controls
#else
namespace Windows.UI.Xaml.Controls
#endif
{
    /// <summary>
    /// Provides a lightweight control for displaying small amounts of text.
    /// </summary>
    /// <example>
    /// <code lang="XAML" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    /// <TextBlock x:Name="TextBlockName" Text="Some text."/>
    /// </code>
    /// <code lang="C#">
    /// TextBlockName.Text = "Some text.";
    /// </code>
    /// </example>
    [ContentProperty("Inlines")]
    public partial class TextBlock : Control //todo: this is supposed to inherit from FrameworkElement but Control has the implementations of FontSize, FontWeight, Foreground, etc. Maybe use an intermediate class between FrameworkElement and Control or add the implementation here too.
    {
        private bool _isTextChanging;

        public TextBlock()
        {
            this.IsTabStop = false; //we want to avoid stopping on this element's div when pressing tab.
            this.Inlines = new InlineCollection(this);
        }

        protected override void OnAfterApplyHorizontalAlignmentAndWidth()
        {
            dynamic style = INTERNAL_HtmlDomManager.GetDomElementStyleForModification(this.INTERNAL_OuterDomElement);
            style.display = "block";
        }

        protected override void OnAfterApplyVerticalAlignmentAndWidth()
        {
            dynamic style = INTERNAL_HtmlDomManager.GetDomElementStyleForModification(this.INTERNAL_OuterDomElement);
            style.display = "block";
        }

        public override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren)
        {
            dynamic div = INTERNAL_HtmlDomManager.CreateDomElementAndAppendIt("div", parentRef, this);
            dynamic divStyle = INTERNAL_HtmlDomManager.GetDomElementStyleForModification(div);
            divStyle.whiteSpace = TextWrapping == TextWrapping.NoWrap ? "pre" : "pre-wrap";
            divStyle.overflow = "hidden"; //keeps the text from overflowing despite the TextBlock's size limitations.
            divStyle.textAlign = "left"; // this is the default value.
            domElementWhereToPlaceChildren = div;
            return div;
        }

        /// <summary>
        /// Get or Set the Text property
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
                                                                                             typeof(string),
                                                                                             typeof(TextBlock),
                                                                                             new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)d;
            if (!textBlock._isTextChanging)
            {
                textBlock._isTextChanging = true;
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(new Run() { Text = (string)e.NewValue });
                textBlock._isTextChanging = false;
            }
        }

        internal void SetTextPropertyNoCallBack(string text)
        {
            if (!this._isTextChanging)
            {
                this._isTextChanging = true;
                this.SetLocalValue(TextProperty, text);
                this._isTextChanging = false;
            }
        }

        #region Properties for formatting (TextAlignment, TextWrapping)

        /// <summary>
        /// Gets or sets how the text should be aligned in the TextBlock.
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the TextAlignment dependency property.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", 
                                                                                                      typeof(TextAlignment), 
                                                                                                      typeof(TextBlock), 
                                                                                                      new PropertyMetadata(TextAlignment.Left) 
                                                                                                      { 
                                                                                                          MethodToUpdateDom = TextAlignment_MethodToUpdateDom,
                                                                                                          CallPropertyChangedWhenLoadedIntoVisualTree = WhenToCallPropertyChangedEnum.IfPropertyIsSet
                                                                                                      });

        static void TextAlignment_MethodToUpdateDom(DependencyObject d, object newValue)
        {
            var textBlock = (TextBlock)d;
            switch ((TextAlignment)newValue)
            {
                case TextAlignment.Center:
                    INTERNAL_HtmlDomManager.GetFrameworkElementOuterStyleForModification(textBlock).textAlign = "center";
                    break;
                case TextAlignment.Left:
                    INTERNAL_HtmlDomManager.GetFrameworkElementOuterStyleForModification(textBlock).textAlign = "left";
                    break;
                case TextAlignment.Right:
                    INTERNAL_HtmlDomManager.GetFrameworkElementOuterStyleForModification(textBlock).textAlign = "right";
                    break;
                case TextAlignment.Justify:
                    INTERNAL_HtmlDomManager.GetFrameworkElementOuterStyleForModification(textBlock).textAlign = "justify";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets or sets how the TextBlock wraps text.
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// Identifies the TextWrapping dependency property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TextBlock), new PropertyMetadata(TextWrapping.NoWrap)
            {
                GetCSSEquivalent = (instance) =>
                {
                    return new CSSEquivalent()
                    {
                        Value = (inst, value) =>
                            {
                                TextWrapping newTextWrapping = (TextWrapping)value;
                                switch (newTextWrapping)
                                {
                                    case TextWrapping.Wrap:
                                        return "pre-wrap"; //wrap + preserve whitespaces
                                    case TextWrapping.NoWrap:
                                    default:
                                        return "pre"; //nowrap + preserve whitespaces
                                }
                            },
                        Name = new List<string> { "whiteSpace" },
                    };
                },
                CallPropertyChangedWhenLoadedIntoVisualTree = WhenToCallPropertyChangedEnum.IfPropertyIsSet
            }
            );

        #endregion

        public InlineCollection Inlines { get; }

        protected internal override void INTERNAL_OnAttachedToVisualTree()
        {
            base.INTERNAL_OnAttachedToVisualTree();
            foreach (Inline child in this.Inlines)
            {
#if REWORKLOADED
                this.AddVisualChild(child);
#else
                INTERNAL_VisualTreeManager.AttachVisualChildIfNotAlreadyAttached(child, this);
#endif
            }
        }

        internal override void UpdateTabIndex(bool isTabStop, int tabIndex)
        {
            // we don't do anything since TextBlock is not supposed to be a Control in the first place
            // and it is not supposed to be counted in tabbing
            return;
        }
#if WORKINPROGRESS

        // There is an implementation for TextTrimming in the shelvesheets
        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register("TextTrimming",
                                                                                                     typeof(TextTrimming),
                                                                                                     typeof(TextBlock),
                                                                                                     new PropertyMetadata(TextTrimming.None));

        /// <summary>
        /// Gets or sets how the TextBlock trims text.
        /// </summary>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public double BaselineOffset { get; private set; }

        public static readonly DependencyProperty CharacterSpacingProperty = DependencyProperty.Register("CharacterSpacing",
                                                                                                         typeof(int),
                                                                                                         typeof(TextBlock),
                                                                                                         new PropertyMetadata(0));
        public int CharacterSpacing
        {
            get { return (int)this.GetValue(CharacterSpacingProperty); }
            set { this.SetValue(CharacterSpacingProperty, value); }
        }
#endif
    }
}
