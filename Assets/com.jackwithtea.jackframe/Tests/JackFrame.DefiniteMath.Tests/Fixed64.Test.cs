using System;
using UnityEngine;
using FixMath.NET;
using NUnit.Framework;

namespace JackFrame.DefiniteMath {

    public class Fixed64Test {

        [Test]
        public void TestFixed64() {

            Fix64 fp1;
            Fix64 fp2;

            // 除
            fp1 = new Fix64(9);
            fp2 = new Fix64(3);
            Assert.That((fp1 / fp2) == new Fix64(3));

            fp1 = new Fix64(9);
            fp2 = new Fix64(2);
            Assert.That(((float)(fp1 / fp2)) == 4.5f);
            Assert.That((fp1 / fp2) == (new Fix64(45) / new Fix64(10)));

            // 加减
            fp1 = new Fix64(9);
            fp2 = new Fix64(2);
            Assert.That((int)(fp1 + fp2) == 11);
            Assert.That((int)(fp1 - fp2) == 7);

            // 乘
            fp1 = new Fix64(9);
            fp2 = new Fix64(2);
            Assert.That((int)(fp1 * fp2) == 18);
            Assert.That((fp1 * fp2) == new Fix64(18));
            Assert.That((fp1 * fp2) == (new Fix64(6) * new Fix64(3)));

        }

    }

}