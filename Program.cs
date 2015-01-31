using NUnit.Core;
using System;
using Thrift.Protocol;
using Thrift.Transport;
using org.mustbe.consulo.execution.testframework.thrift.runner;

namespace consulo_nunit_wrapper {
  class OurEventListener : EventListener {
    private TestInterface.Client myClient;

    private String myLastTest;

    public OurEventListener(TestInterface.Client client) {
      myClient = client;
    }

    public void RunFinished(Exception exception) {
      myClient.runFinished();
    }

    public void RunFinished(TestResult result) {
      myClient.runFinished();
    }

    public void RunStarted(string name, int testCount) {
      myClient.runStarted();
    }


    public void SuiteStarted(TestName testName) {
      myClient.suiteStarted(testName.Name, null);
    }

    public void SuiteFinished(TestResult result) {
      myClient.suiteFinished(result.Name);
    }

    public void TestFinished(TestResult result) {
      if (result.ResultState == ResultState.Ignored) {
        myClient.testIgnored(result.Name, result.Message, result.StackTrace);
      }
      else if (!result.IsSuccess) {
        myClient.testFailed(result.Name, result.Message, result.StackTrace, result.IsError, null, null);
      }
      else {
        myClient.testFinished(result.Name, (long)result.Time);
      }
      myLastTest = null;
    }

    public void TestOutput(TestOutput testOutput) {
      myClient.testOutput(myLastTest, testOutput.Text, testOutput.Type == TestOutputType.Out);
    }

    public void TestStarted(TestName testName) {
      myClient.testStarted(myLastTest = testName.Name, null);
    }

    public void UnhandledException(Exception exception) {

    }
  }

  class Program {
    static void Main(string[] args) {
      try {

        CoreExtensions.Host.InitializeService();

        int port = int.Parse(args[1]);

        TSocket socket = new TSocket("localhost", port);

        TestInterface.Client client = new TestInterface.Client(new TBinaryProtocol(socket));

        socket.Open();

        TestPackage testPackage = new TestPackage(args[0]);
        RemoteTestRunner remoteTestRunner = new RemoteTestRunner();
        remoteTestRunner.Load(testPackage);
        remoteTestRunner.Run(new OurEventListener(client), TestFilter.Empty, false, LoggingThreshold.All);
      }
      catch(Exception e) {
        System.Console.WriteLine(e.Message + "\n" + e.StackTrace);
      }
    }
  }
}
