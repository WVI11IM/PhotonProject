# DRIFTFORGE / DRIFTBOUND

Created by [Jayde Iris Callejas](https://github.com/JaydedCompanion) & [William Hu Seo](https://github.com/WVI11IM/)

### Genre

- Co-op
- 2D Shoot 'em Up
- Resource Management
- Puzzle / Strategy 
- Endless / Arcade
- Survival

### About

- A two-player co-op game where two crew members must coordinate between different roles to manage and operate a spaceship.
- One player pilots the ship and fights enemies to collect resources. The other sustains the ship by managing the collected resources and depositing them in the ship's various sectors. Communication between players is essential.
- In this co-op real time dynamicc the main objective is to survive for the longest time possible.

### Networking

- [Photon PUN](https://www.photonengine.com/pun)
- 2 players per room
- Data sent via RPC (Remote procedure call)
- Captain owns map resources, enemies, ship position and sends item pickup events
- Manager owns inventory UI, card reader input and resource replenishing events
- Both players share match time and ship stats