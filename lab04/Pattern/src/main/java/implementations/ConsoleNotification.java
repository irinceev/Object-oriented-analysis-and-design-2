package implementations;

import interfaces.INotification;

public class ConsoleNotification implements INotification {
    public void send(String message) {
        System.out.println("[Console] " + message);
    }
}