import React, {useState, useEffect} from 'react';
import {Error, Info} from "@material-ui/icons";
import {Grid, Typography} from "@material-ui/core";

export default function Notifications(props) {
    const [notifications, setNotifications] = useState([])

    useEffect(() => {
        props.hubConnection.on("createNotification", (message) => {
            var newNotifications = notifications.slice();
            newNotifications.push(message);
            setNotifications(newNotifications);
            console.log(message);
        });

        props.hubConnection.on("deleteNotification", (message) => {
                setTimeout(() => {
                    var newNotifications = notifications.filter(item => item.name !== message.name);
                    setNotifications(newNotifications);
                }, 2000);
        })
    }, [])

    function renderLevel(level) {
        if (level === "info")
            return <Info color={"primary"}/>
        if (level === "error")
            return <Error color={"error"}/>
    }

    return (
        <div style={{width: "10%", position: "fixed", bottom: 0}}>
            {
                notifications.map(item =>
                    <Grid key={item.id} container spacing={2}>
                        <Grid item>
                            {renderLevel(item.level)}
                        </Grid>
                        <Grid item xs={12} sm container>
                            <Typography>{item.message}</Typography>
                        </Grid>
                    </Grid>
                )
            }
        </div>
    )
}