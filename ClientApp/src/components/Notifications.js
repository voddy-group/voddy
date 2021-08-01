import React, {useState, useEffect} from 'react';
import {Error, Info} from "@material-ui/icons";
import {Grid, Typography} from "@material-ui/core";

export default function Notifications(props) {
    const [notifications, setNotifications] = useState([])
    props.hubConnection.on("notification", (message) => {
        var newNotifications = notifications.slice();
        if (message.method === "delete") {
            newNotifications = newNotifications.filter(item => item.name !== message.name);
        } else if (message.method === "create") {
            newNotifications.push(message);
        }
        setNotifications(newNotifications);
    });

    function renderLevel(level) {
        if (level === "info")
            return <Info color={"primary"}/>
        if (level === "error")
            return <Error color={"primary"}/>
    }

    return (
        <div style={{width: "10%", position: "fixed", bottom: 0}}>
            {
                notifications.map(item =>
                    <Grid container spacing={2}>
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