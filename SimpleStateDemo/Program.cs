using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateDemo
{
    // State Interface
    public interface State
    {
        void handle();
    }

    // Concrete State A
    public class StateA: State
    {
        public void handle()
        {
                Console.WriteLine("Handling request in State A");
        }
    }

    // Concrete State B
    public class StateB : State
    {
        public void handle()
        {
            Console.WriteLine("Handling request in State B");
        }
    }

    // Context Class
    public class Context
    {
        public State CurrentState;

        public void setState(State state)
        {
            this.CurrentState = state;
        }

        public void request()
        {
            CurrentState.handle();
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Context context = new Context();

            State stateA = new StateA();
            State stateB = new StateB();

            context.setState(stateA);
            context.request();

            context.setState(stateB);
            context.request();

            Console.ReadKey();
        }
    }
}
