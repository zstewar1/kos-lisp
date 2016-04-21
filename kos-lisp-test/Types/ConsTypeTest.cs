using NUnit.Framework;
using System;
using System.Collections.Immutable;

using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp.Types {
  [TestFixture]
  public class ConsTypeTest {
    [TearDown]
    public void Cleanup () {
      if (LispInterpreter.ExceptionOccurred)
        LispInterpreter.ClearException();
    }

    [Test]
    public void TestCreate () {
      var car = NumberType.Create(0);
      var cdr = NumberType.Create(1);

      ConsType cons = ConsType.Create(car, cdr);

      Assert.NotNull(cons);
      Assert.NotNull(cons.__class__);
      Assert.AreSame(ConsType.Cons, cons.__class__);
      Assert.Null(cons.__dict__);
    }

    [Test]
    public void TestGet () {
      var car = NumberType.Create(0);
      var cdr = NumberType.Create(1);

      ConsType cons = ConsType.Create(car, cdr);

      // Ensure that the correct Car and Cdr are returned both through direct access and
      // when using ListOperations.
      Assert.AreSame(car, cons.Car);
      Assert.AreSame(cdr, cons.Cdr);

      Assert.AreSame(car, ListOperations.GetCar(cons));
      Assert.AreSame(cdr, ListOperations.GetCdr(cons));
    }

    [Test]
    public void TestCopyCons () {
      LispObject list = NilType.Nil;
      for (int i = 7; i >= 0; i--) {
        list = ConsType.Create(NumberType.Create(i), list);
      }

      LispObject copy = ConsType.Copy(list);
      while (list != NilType.Nil) {
        Assert.AreNotSame(list, copy);
        Assert.AreSame(ListOperations.GetCar(list), ListOperations.GetCar(copy));

        list = ListOperations.GetCdr(list);
        copy = ListOperations.GetCdr(copy);
      }
      Assert.AreSame(list, copy);
    }

    [Test]
    public void TestCopyCons_Nil () {
      LispObject copy = ConsType.Copy(NilType.Nil);
      Assert.AreSame(NilType.Nil, copy);
    }

    [Test]
    public void TestCopyCons_FromICons () {
      LispObject list = NilType.Nil;
      for (int i = 7; i >= 0; i--) {
        list = IConsType.Create(NumberType.Create(i), list);
      }

      LispObject copy = ConsType.Copy(list);

      while (list != NilType.Nil) {
        Assert.IsInstanceOf<ConsType>(copy);
        Assert.AreSame(ConsType.Cons, copy.__class__);
        Assert.AreSame(ListOperations.GetCar(list), ListOperations.GetCar(copy));
        list = ListOperations.GetCdr(list);
        copy = ListOperations.GetCdr(copy);
      }

      Assert.AreSame(list, copy);
    }

    [Test]
    public void TestCopyCons_Mixed () {
      LispObject list =
        IConsType.Create(NumberType.Create(0),
          ConsType.Create(NumberType.Create(1),
            IConsType.Create(NumberType.Create(2),
              NilType.Nil)));

      LispObject copy = ConsType.Copy(list);

      while (list != NilType.Nil) {
        Assert.IsInstanceOf<ConsType>(copy);
        Assert.AreSame(ConsType.Cons, copy.__class__);
        Assert.AreNotSame(copy, list);
        Assert.AreSame(ListOperations.GetCar(list), ListOperations.GetCar(copy));

        list = ListOperations.GetCdr(list);
        copy = ListOperations.GetCdr(copy);
      }
      Assert.AreSame(list, copy);
    }

    [Test]
    public void TestConvertList () {
      var list = ImmutableList.Create(
        NumberType.Create(0),
        NumberType.Create(1),
        NumberType.Create(2),
        NumberType.Create(3));

      LispObject converted = ConsType.ToLispList(list);

      for (int i = 0; i < list.Count; i++) {
        Assert.NotNull(converted);
        Assert.IsInstanceOf<ConsType>(converted);
        Assert.AreSame(ConsType.Cons, converted.__class__);
        Assert.AreSame(list[i], ListOperations.GetCar(converted));
        converted = ListOperations.GetCdr(converted);
      }

      Assert.AreSame(converted, NilType.Nil);
    }

    [Test]
    public void TestConvertList_Trivial () {
      var list = ImmutableList.Create<LispObject>();
      LispObject converted = ConsType.ToLispList(list);
      Assert.AreSame(NilType.Nil, converted);
    }

    [Test]
    public void TestConvertListParams () {
      var list = ImmutableList.Create(
        NumberType.Create(0),
        NumberType.Create(1),
        NumberType.Create(2),
        NumberType.Create(3));

      LispObject converted = ConsType.ToLispList(list[0], list[1], list[2], list[3]);

      for (int i = 0; i < list.Count; i++) {
        Assert.NotNull(converted);
        Assert.IsInstanceOf<ConsType>(converted);
        Assert.AreSame(ConsType.Cons, converted.__class__);
        Assert.AreSame(list[i], ListOperations.GetCar(converted));
        converted = ListOperations.GetCdr(converted);
      }

      Assert.AreSame(converted, NilType.Nil);
    }

    [Test]
    public void TestConvertListParams_Trivial () {
      LispObject converted = ConsType.ToLispList();
      Assert.AreSame(NilType.Nil, converted);
    }

    [Test]
    public void TestSetCar () {
      var n1 = NumberType.Create(0);
      var n2 = NumberType.Create(1);
      var n3 = NumberType.Create(2);

      ConsType cons = ConsType.Create(n1, n2);

      var result = ListOperations.SetCar(cons, n3);

      Assert.AreSame(result, NilType.Nil);
      Assert.False(LispInterpreter.ExceptionOccurred);
      Assert.AreSame(n3, cons.Car);
      Assert.AreSame(n2, cons.Cdr);
    }

    [Test]
    public void TestSetCdr () {
      var n1 = NumberType.Create(0);
      var n2 = NumberType.Create(1);
      var n3 = NumberType.Create(2);

      ConsType cons = ConsType.Create(n1, n2);

      var result = ListOperations.SetCdr(cons, n3);

      Assert.AreSame(result, NilType.Nil);
      Assert.False(LispInterpreter.ExceptionOccurred);
      Assert.AreSame(n1, cons.Car);
      Assert.AreSame(n3, cons.Cdr);
    }
  }
}

