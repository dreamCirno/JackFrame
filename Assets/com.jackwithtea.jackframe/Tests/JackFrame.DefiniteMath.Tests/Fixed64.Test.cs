using System;
using UnityEngine;
using NUnit.Framework;

namespace JackFrame.DefiniteMath {

    public class Fixed64Test {

        [Test]
        public void TestFixed64() {

            Fixed64 fp1;
            Fixed64 fp2;

            // 除
            fp1 = new Fixed64(9);
            fp2 = new Fixed64(3);
            Assert.That((fp1 / fp2) == new Fixed64(3));

            fp1 = new Fixed64(9);
            fp2 = new Fixed64(2);
            Assert.That((fp1 / fp2).ToFloat() == 4.5f);
            Assert.That((fp1 / fp2) == (new Fixed64(45) / new Fixed64(10)));

            // 加减
            fp1 = new Fixed64(9);
            fp2 = new Fixed64(2);
            Assert.That((fp1 + fp2).ToInt() == 11);
            Assert.That((fp1 - fp2).ToInt() == 7);

            // 乘
            fp1 = new Fixed64(9);
            fp2 = new Fixed64(2);
            Assert.That((fp1 * fp2).ToInt() == 18);
            Assert.That((fp1 * fp2) == new Fixed64(18));
            Assert.That((fp1 * fp2) == (new Fixed64(6) * new Fixed64(3)));

        }

    }

}