using System;

namespace voddy.Exceptions.Quartz {
    public class MissingJobException : Exception {
        public MissingJobException() {
        }

        public MissingJobException(string message)
            : base(message) {
        }
    }
}