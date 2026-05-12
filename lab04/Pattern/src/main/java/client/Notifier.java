package client;

import interfaces.INotification;

public class Notifier {
    public static void notify(INotification sender, String message) {
        sender.send(message);
    }
}