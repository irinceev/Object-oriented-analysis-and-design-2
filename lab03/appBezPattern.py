import pygame
import random
import sys
from datetime import datetime

pygame.init()

WIDTH, HEIGHT = 400, 600
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("FAPPY BIRD")
clock = pygame.time.Clock()
message = ""
message_timer = 0
MESSAGE_DURATION = 120
font = pygame.font.SysFont(None, 32)

# ---------------- GAME ----------------

class Game:
    def __init__(self):
        self.reset()

    def reset(self):
        self.bird_y = 300
        self.velocity = 0
        self.pipes = []
        self.spawn_pipe()
        self.score = 0
        self.game_over = False

    def spawn_pipe(self):
        gap = 150
        height = random.randint(100, 400)
        self.pipes.append({
            "x": WIDTH,
            "top": height,
            "bottom": height + gap
        })

    def update(self):
        if self.game_over:
            return

        self.velocity += 0.5
        self.bird_y += self.velocity

        R = 15
        if self.bird_y < R:
            self.bird_y = R
            self.velocity = 0
        if self.bird_y > HEIGHT - R:
            self.bird_y = HEIGHT - R
            self.velocity = 0

        for pipe in self.pipes:
            pipe["x"] -= 3

        if self.pipes[-1]["x"] < 200:
            self.spawn_pipe()

        if self.pipes[0]["x"] < -50:
            self.pipes.pop(0)
            self.score += 1

        bird_x = 100
        for pipe in self.pipes:
            if pipe["x"] < bird_x + R and pipe["x"] + 50 > bird_x - R:
                if self.bird_y - R < pipe["top"] or self.bird_y + R > pipe["bottom"]:
                    self.game_over = True

    def jump(self):
        self.velocity = -8

# ---------------- SAVE MANAGER ----------------

class SaveManager:
    def __init__(self):
        # Слоты хранят сырые словари с данными
        self.slots = [None] * 5

    def save(self, slot, game):
        self.slots[slot] = {
            "bird_y": game.bird_y,
            "velocity": game.velocity,
            "pipes": [p.copy() for p in game.pipes],
            "score": game.score,
            "time": datetime.now().strftime("%H:%M:%S")
        }

    def load(self, slot, game):
        data = self.slots[slot]
        if data is None:
            return
        game.bird_y = data["bird_y"]
        game.velocity = data["velocity"]
        game.pipes = [p.copy() for p in data["pipes"]]
        game.score = data["score"]
        game.game_over = False

    def delete(self, slot):
        self.slots[slot] = None

    def get_info(self, slot):
        data = self.slots[slot]
        if data is None:
            return "Пусто"
        return f"Очков: {data['score']} | {data['time']}"

    def has_save(self, slot):
        return self.slots[slot] is not None

    def latest_slot(self):
        latest_slot = None
        latest_time = None
        for i, data in enumerate(self.slots):
            if data is not None:
                if latest_time is None or data["time"] > latest_time:
                    latest_time = data["time"]
                    latest_slot = i
        return latest_slot

# ---------------- UI ----------------

class Button:
    def __init__(self, text, x, y, w, h):
        self.text = text
        self.rect = pygame.Rect(x, y, w, h)

    def draw(self, highlight=False):
        color = (0, 200, 0) if highlight else (0, 0, 0)
        pygame.draw.rect(screen, color, self.rect, 2)
        label = font.render(self.text, True, (0, 0, 0))
        screen.blit(label, (self.rect.x + 10, self.rect.y + 10))

    def is_clicked(self, pos):
        return self.rect.collidepoint(pos)

# ---------------- INIT ----------------

game = Game()
saves = SaveManager()
state = "menu"

continue_btn = Button("Продолжить", 100, 140, 200, 50)
start_btn = Button("Начать игру", 100, 200, 200, 50)
save_menu_btn = Button("Сохранить игру", 100, 270, 200, 50)
load_menu_btn = Button("Загрузить игру", 100, 340, 200, 50)

slot_buttons = [Button(f"Слот {i+1}", 100, 150 + i*60, 200, 50) for i in range(5)]

restart_btn = Button("Заново", 100, 300, 200, 50)
menu_btn = Button("Главное меню", 100, 370, 200, 50)

# ---------------- LOOP ----------------

while True:
    screen.fill((135, 206, 235))
    mouse_pos = pygame.mouse.get_pos()

    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            sys.exit()

        if event.type == pygame.KEYDOWN:

            if state == "game" and not game.game_over:
                if event.key == pygame.K_1:
                    saves.save(0, game)
                    message = "Сохранено!"
                    message_timer = MESSAGE_DURATION
                if event.key == pygame.K_2:
                    saves.save(1, game)
                    message = "Сохранено!"
                    message_timer = MESSAGE_DURATION
                if event.key == pygame.K_3:
                    saves.save(2, game)
                    message = "Сохранено!"
                    message_timer = MESSAGE_DURATION

                if event.key == pygame.K_q and saves.has_save(0):
                    saves.load(0, game)
                    message = "Загружено!"
                    message_timer = MESSAGE_DURATION
                if event.key == pygame.K_w and saves.has_save(1):
                    saves.load(1, game)
                    message = "Загружено!"
                    message_timer = MESSAGE_DURATION
                if event.key == pygame.K_e and saves.has_save(2):
                    saves.load(2, game)
                    message = "Загружено!"
                    message_timer = MESSAGE_DURATION

            if state == "game" and not game.game_over and event.key == pygame.K_SPACE:
                game.jump()

            if event.key == pygame.K_ESCAPE:
                state = "menu"

        if event.type == pygame.MOUSEBUTTONDOWN:
            pos = pygame.mouse.get_pos()

            if state == "menu":
                if start_btn.is_clicked(pos):
                    game.reset()
                    state = "game"
                if save_menu_btn.is_clicked(pos):
                    state = "save_menu"
                if load_menu_btn.is_clicked(pos):
                    state = "load_menu"
                if continue_btn.is_clicked(pos):
                    slot = saves.latest_slot()
                    if slot is not None:
                        saves.load(slot, game)
                        message = "Загружено!"
                        message_timer = MESSAGE_DURATION
                        state = "game"

            elif state == "save_menu":
                for i, btn in enumerate(slot_buttons):
                    if btn.is_clicked(pos):
                        if event.button == 1:
                            saves.save(i, game)
                            message = "Сохранено!"
                            message_timer = MESSAGE_DURATION
                        elif event.button == 3:
                            saves.delete(i)

            elif state == "load_menu":
                for i, btn in enumerate(slot_buttons):
                    if btn.is_clicked(pos):
                        if saves.has_save(i):
                            saves.load(i, game)
                            message = "Загружено!"
                            message_timer = MESSAGE_DURATION
                            state = "game"

            elif state == "game" and game.game_over:
                if restart_btn.is_clicked(pos):
                    game.reset()
                if menu_btn.is_clicked(pos):
                    state = "menu"

    if state == "menu":
        title = font.render("МЕНЮ", True, (0, 0, 0))
        screen.blit(title, (160, 100))
        continue_btn.draw()
        start_btn.draw()
        save_menu_btn.draw()
        load_menu_btn.draw()

    elif state == "save_menu":
        title = font.render("СОХРАНЕНИЕ", True, (0, 0, 0))
        screen.blit(title, (120, 50))
        for i, btn in enumerate(slot_buttons):
            highlight = btn.rect.collidepoint(mouse_pos)
            btn.draw(highlight)
            info = font.render(saves.get_info(i), True, (0, 0, 0))
            screen.blit(info, (100, 150 + i*60 + 30))

    elif state == "load_menu":
        title = font.render("ЗАГРУЗКА", True, (0, 0, 0))
        screen.blit(title, (120, 50))
        for i, btn in enumerate(slot_buttons):
            highlight = btn.rect.collidepoint(mouse_pos)
            btn.draw(highlight)
            info = font.render(saves.get_info(i), True, (0, 0, 0))
            screen.blit(info, (100, 150 + i*60 + 30))

    elif state == "game":
        if not game.game_over:
            game.update()

        pygame.draw.circle(screen, (255, 0, 0), (100, int(game.bird_y)), 15)
        for pipe in game.pipes:
            pygame.draw.rect(screen, (0, 255, 0), (pipe["x"], 0, 50, pipe["top"]))
            pygame.draw.rect(screen, (0, 255, 0), (pipe["x"], pipe["bottom"], 50, HEIGHT))
        score_text = font.render(f"Очки: {game.score}", True, (0, 0, 0))
        screen.blit(score_text, (10, 10))

        if game.game_over:
            overlay = pygame.Surface((WIDTH, HEIGHT))
            overlay.set_alpha(180)
            overlay.fill((0, 0, 0))
            screen.blit(overlay, (0, 0))
            text1 = font.render("СМЕРТЬ", True, (255, 255, 255))
            text2 = font.render(f"Очки: {game.score}", True, (255, 255, 255))
            screen.blit(text1, (120, 200))
            screen.blit(text2, (130, 240))
            restart_btn.draw()
            menu_btn.draw()

        hint1 = font.render("1-2-3 сохранить", True, (0, 0, 0))
        hint2 = font.render("Q-W-E загрузить", True, (0, 0, 0))
        screen.blit(hint1, (10, 40))
        screen.blit(hint2, (10, 70))

    if message_timer > 0:
        msg_label = font.render(message, True, (0, 255, 0))
        screen.blit(msg_label, (WIDTH - 140, 10))
        message_timer -= 1

    pygame.display.flip()
    clock.tick(60)