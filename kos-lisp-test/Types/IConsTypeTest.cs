using NUnit.Framework;
using System;
using System.Collections.Immutable;

using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp.Types {
  [TestFixture]
  public class IConsTypeTest {
    [TearDown]
    public void Cleanup () {
      if (LispInterpreter.ExceptionOccurred)
        LispInterpreter.ClearException();
    }

    [Test]
    public void TestCreate () {
      var car = NumberType.Create(0);
      var cdr = NumberType.Create(1);

      IConsType icons = IConsType.Create(car, cdr);

      Assert.NotNull(icons);
      Assert.NotNull(icons.__class__);
      Assert.AreEqual(IConsType.ICons, icons.__class__);
      Assert.Null(icons.__dict__);
    }

    [Test]
    public void TestGet () {
      var car = NumberType.Create(0);
      var cdr = NumberType.Create(1);

      IConsType icons = IConsType.Create(car, cdr);

      // Ensure that the correct Car and Cdr are returned both through direct access and
      // when using ListOperations.
      Assert.AreSame(car, icons.Car);
      Assert.AreSame(cdr, icons.Cdr);

      Assert.AreSame(car, ListOperations.GetCar(icons));
      Assert.AreSame(cdr, ListOperations.GetCdr(icons));
    }

    [Test]
    public void TestCopyIcons () {
      LispObject list = NilType.Nil;
      for (int i = 7; i >= 0; i--) {
        list = IConsType.Create(NumberType.Create(i), list);
      }

      LispObject copy = IConsType.Copy(list);
      do {
        Assert.AreSame(list, copy);

        list = ListOperations.GetCdr(list);
        copy = ListOperations.GetCdr(copy);
      } while (list != NilType.Nil);
    }

    [Test]
    public void TestCopyIcons_Nil () {
      LispObject copy = IConsType.Copy(NilType.Nil);
      Assert.AreSame(NilType.Nil, copy);
    }

    [Test]
    public void TestCopyIcons_FromCons () {
      LispObject list = NilType.Nil;
      for (int i = 7; i >= 0; i--) {
        list = ConsType.Create(NumberType.Create(i), list);
      }

      LispObject copy = IConsType.Copy(list);

      while (list != NilType.Nil) {
        Assert.IsInstanceOf<IConsType>(copy);
        Assert.AreSame(IConsType.ICons, copy.__class__);
        Assert.AreSame(ListOperations.GetCar(list), ListOperations.GetCar(copy));
        list = ListOperations.GetCdr(list);
        copy = ListOperations.GetCdr(copy);
      }

      Assert.AreSame(list, copy);
    }

    [Test]
    public void TestCopyIcons_Mixed () {
      LispObject list =
        IConsType.Create(NumberType.Create(0),
          ConsType.Create(NumberType.Create(1),
            IConsType.Create(NumberType.Create(2),
              NilType.Nil)));

      LispObject copy = IConsType.Copy(list);

      Assert.IsInstanceOf<IConsType>(copy);
      Assert.AreSame(IConsType.ICons, copy.__class__);
      Assert.AreNotSame(copy, list);
      Assert.AreEqual(ListOperations.GetCar(list), ListOperations.GetCar(copy));

      list = ListOperations.GetCdr(list);
      copy = ListOperations.GetCdr(copy);

      Assert.IsInstanceOf<IConsType>(copy);
      Assert.AreEqual(IConsType.ICons, copy.__class__);
      Assert.AreEqual(ListOperations.GetCar(list), ListOperations.GetCar(copy));

      list = ListOperations.GetCdr(list);
      copy = ListOperations.GetCdr(copy);

      Assert.AreSame(list, copy);
    }

    [Test]
    public void TestConvertList () {
      var list = ImmutableList.Create(
        NumberType.Create(0),
        NumberType.Create(1),
        NumberType.Create(2),
        NumberType.Create(3));

      LispObject converted = IConsType.ToLispTuple(list);

      for (int i = 0; i < list.Count; i++) {
        Assert.NotNull(converted);
        Assert.IsInstanceOf<IConsType>(converted);
        Assert.AreSame(IConsType.ICons, converted.__class__);
        Assert.AreSame(list[i], ListOperations.GetCar(converted));
        converted = ListOperations.GetCdr(converted);
      }

      Assert.AreSame(converted, NilType.Nil);
    }

    [Test]
    public void TestConvertList_Trivial () {
      var list = ImmutableList.Create<LispObject>();
      LispObject converted = IConsType.ToLispTuple(list);
      Assert.AreSame(NilType.Nil, converted);
    }

    [Test]
    public void TestConvertListParams () {
      var list = ImmutableList.Create(
        NumberType.Create(0),
        NumberType.Create(1),
        NumberType.Create(2),
        NumberType.Create(3));

      LispObject converted = IConsType.ToLispTuple(list[0], list[1], list[2], list[3]);

      for (int i = 0; i < list.Count; i++) {
        Assert.NotNull(converted);
        Assert.IsInstanceOf<IConsType>(converted);
        Assert.AreSame(IConsType.ICons, converted.__class__);
        Assert.AreSame(list[i], ListOperations.GetCar(converted));
        converted = ListOperations.GetCdr(converted);
      }

      Assert.AreSame(converted, NilType.Nil);
    }

    [Test]
    public void TestConvertListParams_Trivial () {
      LispObject converted = IConsType.ToLispTuple();
      Assert.AreSame(NilType.Nil, converted);
    }

    [Test]
    public void TestSetCar () {
      var n1 = NumberType.Create(0);
      var n2 = NumberType.Create(1);
      var n3 = NumberType.Create(2);

      IConsType cons = IConsType.Create(n1, n2);

      var result = ListOperations.SetCar(cons, n3);

      Assert.Null(result);
      Assert.True(LispInterpreter.CheckException(ExceptionType.TypeError));
      Assert.AreSame(n1, cons.Car);
      Assert.AreSame(n2, cons.Cdr);
    }

    [Test]
    public void TestSetCdr () {
      var n1 = NumberType.Create(0);
      var n2 = NumberType.Create(1);
      var n3 = NumberType.Create(2);

      IConsType cons = IConsType.Create(n1, n2);

      var result = ListOperations.SetCdr(cons, n3);

      Assert.Null(result);
      Assert.True(LispInterpreter.CheckException(ExceptionType.TypeError));
      Assert.AreSame(n1, cons.Car);
      Assert.AreSame(n2, cons.Cdr);
    }
  }
}

