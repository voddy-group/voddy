import React, {useState, useEffect} from 'react';
import {Error, Info} from "@material-ui/icons";
import {Grid, Typography} from "@material-ui/core";

export default function SideNotifications(props) {
    const [notifications, setNotifications] = useState([])

    useEffect(() => {
        props.hubConnection.on("createNotification", (message) => {
            if (message.position === 1) {
                var newNotifications = notifications.slice();
                newNotifications.push(message);
                setNotifications(newNotifications);
                console.log(message);
            }
        });

        props.hubConnection.on("deleteNotification", (message) => {
            setTimeout(() => {
                var newNotifications = notifications.filter(item => item.id !== message.id);
                setNotifications(newNotifications);
            }, 2000);
        })
    }, [])

    function renderLevel(level) {
        if (level === 0 || level === 1)
            return <Info color={"primary"}/>
        if (level === 3)
            return <Error color={"error"}/>
    }

    return (
        <div style={{width: "10%", position: "fixed", bottom: 0}}>
            {
                notifications.map(item =>
                    <Grid key={item.id} container spacing={2}>
                        <Grid item>
                            {renderLevel(item.severity)}
                        </Grid>
                        <Grid item xs={12} sm container>
                            <Typography>{item.description}</Typography>
                        </Grid>
                    </Grid>
                )
            }
        </div>
    )
}