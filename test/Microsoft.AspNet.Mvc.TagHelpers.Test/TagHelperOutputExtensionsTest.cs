// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class TagHelperOutputExtensionsTest
    {
        [Theory]
        [InlineData("hello", "world")]
        [InlineData("HeLlO", "wOrLd")]
        public void CopyHtmlAttribute_CopiesOriginalAttributes(string attributeName, string attributeValue)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());
            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributes
                {
                    { attributeName, attributeValue }
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.Append("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            var expectedAttribute = new TagHelperAttribute(attributeName, attributeValue);

            // Act
            tagHelperOutput.CopyHtmlAttribute("hello", tagHelperContext);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(expectedAttribute, attribute);
        }

        [Fact]
        public void CopyHtmlAttribute_DoesNotOverrideAttributes()
        {
            // Arrange
            var attributeName = "hello";
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes()
                {
                    { attributeName, "world2" }
                });
            var expectedAttribute = new TagHelperAttribute(attributeName, "world2");
            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributes
                {
                    { attributeName, "world" }
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.Append("Something Else");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            // Act
            tagHelperOutput.CopyHtmlAttribute(attributeName, tagHelperContext);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(expectedAttribute, attribute);
        }

        [Fact]
        public void CopyHtmlAttribute_ThrowsWhenUnknownAttribute()
        {
            // Arrange
            var invalidAttributeName = "hello2";
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());
            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributes
                {
                    { "hello", "world" }
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.Append("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => tagHelperOutput.CopyHtmlAttribute(invalidAttributeName, tagHelperContext),
                "attributeName",
                "The attribute 'hello2' does not exist in the TagHelperContext.");
        }

        [Fact]
        public void RemoveRange_RemovesProvidedAttributes()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes()
                {
                    { "route-Hello", "World" },
                    { "Route-I", "Am" }
                });
            var expectedAttribute = new TagHelperAttribute("type", "btn");
            tagHelperOutput.Attributes.Add(expectedAttribute);
            var attributes = tagHelperOutput.FindPrefixedAttributes("route-");

            // Act
            tagHelperOutput.RemoveRange(attributes);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(expectedAttribute, attribute);
        }

        [Fact]
        public void FindPrefixedAttributes_ReturnsEmpty_AttributeListIfNoAttributesPrefixed()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes()
                {
                    { "routeHello", "World" },
                    { "Routee-I", "Am" }
                });

            // Act
            var attributes = tagHelperOutput.FindPrefixedAttributes("route-");

            // Assert
            Assert.Empty(attributes);
            var attribute = Assert.Single(tagHelperOutput.Attributes, attr => attr.Name.Equals("routeHello"));
            Assert.Equal(attribute.Value, "World");
            attribute = Assert.Single(tagHelperOutput.Attributes, attr => attr.Name.Equals("Routee-I"));
            Assert.Equal(attribute.Value, "Am");
        }

        [Fact]
        public void MergeAttributes_DoesNotReplace_TagHelperOutputAttributeValues()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());
            var expectedAttribute = new TagHelperAttribute("type", "btn");
            tagHelperOutput.Attributes.Add(expectedAttribute);

            var tagBuilder = new TagBuilder("p", new CommonTestEncoder());
            tagBuilder.Attributes.Add("type", "hello");

            // Act
            tagHelperOutput.MergeAttributes(tagBuilder);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(expectedAttribute, attribute);
        }

        [Fact]
        public void MergeAttributes_AppendsClass_TagHelperOutputAttributeValues()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());
            tagHelperOutput.Attributes.Add("class", "Hello");

            var tagBuilder = new TagBuilder("p", new CommonTestEncoder());
            tagBuilder.Attributes.Add("class", "btn");

            var expectedAttribute = new TagHelperAttribute("class", "Hello btn");

            // Act
            tagHelperOutput.MergeAttributes(tagBuilder);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(expectedAttribute, attribute);
        }

        [Theory]
        [InlineData("class", "CLAss")]
        [InlineData("ClaSS", "class")]
        [InlineData("ClaSS", "cLaSs")]
        public void MergeAttributes_AppendsClass_TagHelperOutputAttributeValues_IgnoresCase(
            string originalName, string updateName)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());
            tagHelperOutput.Attributes.Add(originalName, "Hello");

            var tagBuilder = new TagBuilder("p", new CommonTestEncoder());
            tagBuilder.Attributes.Add(updateName, "btn");

            // Act
            tagHelperOutput.MergeAttributes(tagBuilder);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(new TagHelperAttribute(originalName, "Hello btn"), attribute);
        }

        [Fact]
        public void MergeAttributes_DoesNotEncode_TagHelperOutputAttributeValues()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());

            var tagBuilder = new TagBuilder("p", new CommonTestEncoder());
            var expectedAttribute = new TagHelperAttribute("visible", "val < 3");
            tagBuilder.Attributes.Add("visible", "val < 3");

            // Act
            tagHelperOutput.MergeAttributes(tagBuilder);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(expectedAttribute, attribute);
        }

        [Fact]
        public void MergeAttributes_CopiesMultiple_TagHelperOutputAttributeValues()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());

            var tagBuilder = new TagBuilder("p", new CommonTestEncoder());
            var expectedAttribute1 = new TagHelperAttribute("class", "btn");
            var expectedAttribute2 = new TagHelperAttribute("class2", "btn");
            tagBuilder.Attributes.Add("class", "btn");
            tagBuilder.Attributes.Add("class2", "btn");

            // Act
            tagHelperOutput.MergeAttributes(tagBuilder);

            // Assert
            Assert.Equal(2, tagHelperOutput.Attributes.Count);
            var attribute = Assert.Single(tagHelperOutput.Attributes, attr => attr.Name.Equals("class"));
            Assert.Equal(expectedAttribute1.Value, attribute.Value);
            attribute = Assert.Single(tagHelperOutput.Attributes, attr => attr.Name.Equals("class2"));
            Assert.Equal(expectedAttribute2.Value, attribute.Value);
        }

        [Fact]
        public void MergeAttributes_Maintains_TagHelperOutputAttributeValues()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());
            var expectedAttribute = new TagHelperAttribute("class", "btn");
            tagHelperOutput.Attributes.Add(expectedAttribute);

            var tagBuilder = new TagBuilder("p", new CommonTestEncoder());

            // Act
            tagHelperOutput.MergeAttributes(tagBuilder);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(expectedAttribute, attribute);
        }

        [Fact]
        public void MergeAttributes_Combines_TagHelperOutputAttributeValues()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                attributes: new TagHelperAttributes());
            var expectedOutputAttribute = new TagHelperAttribute("class", "btn");
            tagHelperOutput.Attributes.Add(expectedOutputAttribute);

            var tagBuilder = new TagBuilder("p", new CommonTestEncoder());
            var expectedBuilderAttribute = new TagHelperAttribute("for", "hello");
            tagBuilder.Attributes.Add("for", "hello");

            // Act
            tagHelperOutput.MergeAttributes(tagBuilder);

            // Assert
            Assert.Equal(tagHelperOutput.Attributes.Count, 2);
            var attribute = Assert.Single(tagHelperOutput.Attributes, attr => attr.Name.Equals("class"));
            Assert.Equal(expectedOutputAttribute.Value, attribute.Value);
            attribute = Assert.Single(tagHelperOutput.Attributes, attr => attr.Name.Equals("for"));
            Assert.Equal(expectedBuilderAttribute.Value, attribute.Value);
        }
    }
}
