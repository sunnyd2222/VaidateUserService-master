"# ValidateUserService" 

1) machine PLC-controller connected with RFID reader
2) machine sends login-id via opc to server
3) at server deployed CaSecurity - asp.net app witch database of all users
4) at server ValidateUserService running:
    read login id via opc ua, 
    check if exists in local db, 
    if yes return to opc permision level and message-code that PLC controller could assign privilages or refuse user access to machine,
    service scallable to lot of machines connected - each machine each instance logic
