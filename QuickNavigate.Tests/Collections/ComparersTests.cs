// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using System.Linq;
using ASCompletion.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickNavigate.Collections;
using QuickNavigate.Forms;

namespace QuickNavigate.Tests.Collections
{
    [TestClass]
    public class ComparersTests
    {
        [TestMethod]
        public void TestSortNameAndPackage0()
        {
            var matches = new List<string>(new[]
            {
                "ru.crazypanda.core.display.BaseSprite",
                "ru.crazypanda.core.display.MainResourceSprite",
                "ru.crazypanda.core.display.OctopusResourceSprite",
                "ru.crazypanda.core.display.ResourceSprite",
                "ru.crazypanda.core.display.gui.ScaleGridSprite",
                "away3d.animators.SpriteSheetAnimationSet",
                "away3d.animators.SpriteSheetAnimator",
                "away3d.animators.data.SpriteSheetAnimationFrame",
                "away3d.animators.nodes.ISpriteSheetAnimationNode",
                "away3d.animators.nodes.ParticleSpriteSheetNode",
                "away3d.animators.nodes.SpriteSheetClipNode",
                "away3d.animators.states.ISpriteSheetAnimationState",
                "away3d.animators.states.ParticleSpriteSheetState",
                "away3d.animators.states.SpriteSheetAnimationState",
                "away3d.entities.Sprite3D",
                "away3d.materials.SpriteSheetMaterial",
                "away3d.tools.helpers.SpriteSheetHelper",
                "mx.core.FlexSprite",
                "mx.core.SpriteAsset",
                "starling.display.Sprite",
                "flash.display.Sprite",
                "c.Sprite",
                "a.Sprite",
                "b.Sprite"
            });
            var nodes = matches.Select(match => new ClassNode(new ClassModel {InFile = FileModel.Ignore}, 0)
            {
                Package = match.Substring(0, match.LastIndexOf(".")),
                Name = match.Substring(match.LastIndexOf(".") + 1)
            }).ToList();
            var nodes0 = nodes.Where(node => node.Name.ToLower() == "sprite").ToList();
            var nodes1 = nodes.Where(node => node.Name.ToLower() != "sprite" && node.Name.ToLower().StartsWith("sprite")).ToList();
            var nodes2 = nodes.Where(node => node.Name.ToLower() != "sprite" && !node.Name.ToLower().StartsWith("sprite")).ToList();
            nodes0.Sort(TypeExplorerNodeComparer.Package);
            Assert.AreEqual("Sprite", nodes0[0].Name);
            Assert.AreEqual("a", nodes0[0].Package);
            Assert.AreEqual("Sprite", nodes0[1].Name);
            Assert.AreEqual("b", nodes0[1].Package);
            Assert.AreEqual("Sprite", nodes0[2].Name);
            Assert.AreEqual("c", nodes0[2].Package);
            Assert.AreEqual("Sprite", nodes0[3].Name);
            Assert.AreEqual("flash.display", nodes0[3].Package);
            Assert.AreEqual("Sprite", nodes0[4].Name);
            Assert.AreEqual("starling.display", nodes0[4].Package);
            nodes1.Sort(TypeExplorerNodeComparer.NameIgnoreCase);
            Assert.AreEqual("Sprite3D", nodes1[0].Name);
            Assert.AreEqual("SpriteAsset", nodes1[1].Name);
            nodes2.Sort(TypeExplorerNodeComparer.NamePackageIgnoreCase);
            Assert.AreEqual("BaseSprite", nodes2[0].Name);
            Assert.AreEqual("FlexSprite", nodes2[1].Name);
        }
    }
}