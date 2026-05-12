package implementations;

import interfaces.INotification;
import java.net.*;

public class TelegramNotification implements INotification {
    private String botToken = "8356898265:AAE6_hADT3UnTdxObiAcCou1jD2WrczqSZE";
    private String chatId = "332931052";

    public void send(String message) {
        try {
            String urlStr = "https://api.telegram.org/bot"
                    + botToken
                    + "/sendMessage?chat_id="
                    + chatId
                    + "&text=" + URLEncoder.encode(message, "UTF-8");
            URL url = new URL(urlStr);
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            conn.getInputStream();
            System.out.println("Telegram sent");
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}