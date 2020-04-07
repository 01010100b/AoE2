import msgpackrpc
import time
import sys

# GameRunner port game_type map_type map_size record player1_name player1_team player1_civ player2_name ... 

teams = [0, 0, 0, 0, 0, 0, 0, 0]

def game_running():

    team0 = 0
    team1 = 0
    team2 = 0
    team3 = 0
    team4 = 0
    players_in_game = 0
    team_max = 0
    
    for i in range(8):

        if autogame.call('GetPlayerAlive', i + 1):

            players_in_game += 1

            if teams[i] == 0:
                team0 += 1

            if teams[i] == 1:
                team1 += 1
                if team1 > team_max:
                    team_max = team1

            if teams[i] == 2:
                team2 += 1
                if team2 > team_max:
                    team_max = team2

            if teams[i] == 3:
                team3 += 1
                if team3 > team_max:
                    team_max = team3

            if teams[i] == 4:
                team4 += 1
                if team4 > team_max:
                    team_max = team4

    finished = True

    if team0 > 1:
        finished = False

    if players_in_game > team_max:
        finished = False

    return not finished

autogame = msgpackrpc.Client(msgpackrpc.Address("127.0.0.1", int(sys.argv[1])))

autogame.call('ResetGameSettings')                    # usually reset the settings to make sure everything is valid

autogame.call('SetRunFullSpeed', False)                # run the game logic as fast as possible
autogame.call('SetRunUnfocused', True)                # allow the game to run while minimized
autogame.call('SetWindowMinimized', True)
autogame.call('SetUseInGameResolution', False)
autogame.call('SetGameRevealMap', 0)
autogame.call('SetGameDifficulty', 1)
autogame.call('SetGameStartingAge', 0)
autogame.call('SetGameTeamsLocked', True)
autogame.call('SetGameTeamsTogether', True)

autogame.call('SetGameVictoryType', 1, 0)

autogame.call('SetGameType', int(sys.argv[2]))
autogame.call('SetGameMapType', int(sys.argv[3]))
autogame.call('SetGameMapSize', int(sys.argv[4]))
if sys.argv[5] == "True":
    autogame.call('SetGameRecorded', True)
else:
    autogame.call('SetGameRecorded', False)

for i in range(8):
    index = 6 + (i * 3)
    name = sys.argv[index].replace("%20", " ")
    team = int(sys.argv[index + 1])
    teams[i] = team
    civ = int(sys.argv[index + 2])

    if name == "Closed":
        autogame.call('SetPlayerClosed', i + 1)

    else:
        autogame.call('SetPlayerComputer', i + 1, name)
        autogame.call('SetPlayerTeam', i + 1, team)
        autogame.call('SetPlayerCivilization', i + 1, civ)
    
autogame.call('StartGame')                     # start the match

win1 = 0
win2 = 0
win3 = 0
win4 = 0
win5 = 0
win6 = 0
win7 = 0
win8 = 0
draws = 0

print("Started game", flush = True)

draw = False
while game_running():  # wait until the game has finished
    time.sleep(3.0)
    if autogame.call('GetGameTime') > 3 * 60 * 60:	# if a game has been running for 3 hours, declare it a draw
        draws += 1
        draw = True
        print("Finished game: draw", flush = True)
        break

if not draw:
    winner = autogame.call('GetWinningPlayer')
    winners = autogame.call('GetWinningPlayers')

    if 1 in winners:
        win1 += 1
    if 2 in winners:
        win2 += 1
    if 3 in winners:
        win3 += 1
    if 4 in winners:
        win4 += 1
    if 5 in winners:
        win5 += 1
    if 6 in winners:
        win6 += 1
    if 7 in winners:
        win7 += 1
    if 8 in winners:
        win8 += 1

    print("Finished game, winner: " + str(winner), flush = True)

autogame.call('QuitGame')
print("Result: " + str(win1) + " " + str(win2) + " " + str(win3) + " " + str(win4) + " " + str(win5) + " " + str(win6) + " " + str(win7) + " " + str(win8), flush = True)

time.sleep(10.0)