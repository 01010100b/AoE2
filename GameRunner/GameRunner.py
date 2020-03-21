import msgpackrpc
import time
import sys

# GameRunner number_of_games game_type map_type map_size player1_name player1_team player1_civ player2_name ... 

print('Number of arguments:', len(sys.argv), 'arguments.')
print('Argument List:', str(sys.argv))

# this script repeatedly plays games and marks down the winners
autogame = msgpackrpc.Client(msgpackrpc.Address("127.0.0.1", 64720))

def launch_game():
    autogame.call('ResetGameSettings')                    # usually reset the settings to make sure everything is valid

    autogame.call('SetRunFullSpeed', False)                # run the game logic as fast as possible
    autogame.call('SetRunUnfocused', True)                # allow the game to run while minimized
    autogame.call('SetWindowMinimized', True)
    autogame.call('SetUseInGameResolution', False)

    autogame.call('SetGameRevealMap', 0)
    autogame.call('SetGameDifficulty', 1)
    autogame.call('SetGameStartingAge', 0)
    autogame.call('SetGameTeamsLocked', True)

    autogame.call('SetGameType', int(sys.argv[2]))
    autogame.call('SetGameMapType', int(sys.argv[3]))
    autogame.call('SetGameMapSize', int(sys.argv[4]))

    for i in range(8):
        index = 5 + (i * 3)
        name = sys.argv[index]
        team = int(sys.argv[index + 1])
        civ = int(sys.argv[index + 2])

        if name == "Closed":
            autogame.call('SetPlayerClosed', i + 1)

        else:
            autogame.call('SetPlayerComputer', i + 1, name)
            autogame.call('SetPlayerTeam', i + 1, team)
            autogame.call('SetPlayerCivilization', i + 1, civ)
    
    return autogame.call('StartGame')                     # start the match


max_games = int(sys.argv[1])
num_games = 0
win1 = 0
win2 = 0
win3 = 0
win4 = 0
win5 = 0
win6 = 0
win7 = 0
win8 = 0
draws = 0

while num_games < max_games:

    if not launch_game():
        break

    num_games += 1
    print("Started game " + str(num_games))

    seconds_passed = 0
    draw = False
    while autogame.call('GetGameInProgress'):  # wait until the game has finished
        time.sleep(1.0)
        seconds_passed += 1
        if seconds_passed > 600:	# if a game has been simulated for more than 10 minutes, declare it a draw
            draws += 1
            draw = True
            print("Finished game " + str(num_games) + ": draw.")
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

        print("Finished game " + str(num_games) + ", winner: " + str(winner))

    autogame.call('QuitGame')
    print("Result: " + str(num_games) + "/ " + str(win1) + " " + str(win2) + " " + str(win3) + " " + str(win4) + " " + str(win5) + " " + str(win6) + " " + str(win7) + " " + str(win8))

