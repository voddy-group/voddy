import React, {useEffect, useState} from "react";
import {
    Badge,
    Box,
    IconButton,
    ListItemIcon,
    ListItemText,
    Menu,
    MenuItem,
    MenuList,
    Paper, Snackbar,
    Typography
} from "@material-ui/core";
import {Delete, Info, MailOutline, MovieCreation} from "@material-ui/icons";
import {Alert} from "@material-ui/lab";


export default function MainNotification(props) {
    const [anchorEl, setAnchorEl] = useState(null);
    const [error, setError] = useState(false);
    const menuOpen = Boolean(anchorEl);
    const [notificationList, setNotificationList] = useState([]);

    useEffect(() => {
        getNotifications();
    }, [])

    useEffect(() => {
        props.hubConnection.on("createNotification", (message) => {
            if (message.position === 0) {
                var newNotifications = notificationList.slice();
                newNotifications.push(message);
                setNotificationList(newNotifications);
                console.log(message);
            }
        });
    }, [])

    async function getNotifications() {
        const request = await fetch('notifications?position=Main',
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (request.ok) {
            var response = await request.json();
            setNotificationList(response);
        }
    }

    function handleBadgeClick(event) {
        setAnchorEl(event.currentTarget);
    }

    function handleMenuClose() {
        setAnchorEl(null);
    }
    
    function handleRemoveFromNotificationArray(uuid) {
        for (var x = 0; x < notificationList.length; x++) {
            if (notificationList[x].uuid === uuid) {
                var newNotificationList = [...notificationList];
                newNotificationList.splice(x, 1);
                setNotificationList(newNotificationList);
                
                removeNotificationRequest(uuid);
            }
        }
    }
    
    async function removeNotificationRequest(uuid) {
        const request = await fetch('notifications?uuid=' + uuid,
            {
                method: 'delete',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        
        if (!request.ok) {
            setError(true);
        }
    }

    return (
        <>
            <Badge
                color={"primary"}
                badgeContent={notificationList.length}
                onClick={handleBadgeClick}
                aria-haspopup={true}
                aria-expanded={menuOpen ? 'true' : undefined}>
                <MailOutline color={"action"}/>
            </Badge>
            <Menu
                id={"menu"}
                open={menuOpen}
                onClose={handleMenuClose}
                anchorEl={anchorEl}>
                <Paper
                    style={{width: 320, maxWidth: "100%"}}>
                    <Badge>
                        <MailOutline color={"action"}/>
                    </Badge>
                    <MenuList>
                        {notificationList.map((notification) => (
                            <MenuItem id={notification.uuid}>
                                <ListItemIcon>
                                    <Info fontSize={"small"}/>
                                </ListItemIcon>
                                <ListItemText>{notification.description}</ListItemText>
                                <IconButton>
                                    <Delete onClick={() => handleRemoveFromNotificationArray(notification.uuid)} />
                                </IconButton>
                            </MenuItem>
                        ))}
                    </MenuList>
                </Paper>
            </Menu>
            <Snackbar 
            open={error}
            autoHideDuration={10000}>
                <Alert severity={"error"}>
                    Backend error.
                </Alert>
            </Snackbar>
        </>
    )
}