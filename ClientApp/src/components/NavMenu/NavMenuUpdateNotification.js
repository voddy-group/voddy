import React, {useEffect, useState} from "react";
import {Collapse, List, ListItem, ListItemText, makeStyles} from "@material-ui/core";
import {Link} from "react-router-dom";
import WarningIcon from '@material-ui/icons/Warning';
import {Error} from "@material-ui/icons";

const styles = makeStyles((theme) => ({
    root: {
        height: "100%",
        width: "10%",
        position: "fixed"
    },
    nested: {
        width: "10%",
        paddingLeft: theme.spacing(2)
    }
}))

export default function NavMenuUpdateNotification(props) {
    const [dropDown, setDropDown] = useState(false);
    const [notifications, setNotifications] = useState([]);
    const [error, setError] = useState(false);
    const [warning, setWarning] = useState(false);
    const classes = styles();

    useEffect(() => {
        getUpdates();
    }, [])

    useEffect(() => {
        props.hubConnection.on("updateFound", () => {
            setWarning(true);
        });

        props.hubConnection.on("noConnection", (item) => {
            if (item == "true") {
                setError(true);
            } else {
                setError(false);
            }
        })
    }, [])

    async function getUpdates() {
        const request = await fetch('internal/variables',
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            })

        if (request.ok) {
            var response = await request.json();
            for (var x = 0; x < response.length; x++) {
                switch (response[x].key) {
                    case "updateAvailable":
                        if (response[x].value == "True") {
                            setWarning(true);
                        } else {
                            setWarning(false);
                        }
                        break;
                    case "connectionError":
                        if (response[x].value == "True") {
                            setError(true);
                        } else {
                            setError(false);
                        }
                        break;
                }
            }
        }
    }

    function setNotification() {
        if (error) {
            return <Error color={"error"} />
        } else if (warning) {
            return <WarningIcon color={"secondary"} />
        }
    }

    function toggleDropDown() {
        setDropDown(prevState => !prevState);
    }

    return (
        <>
            <ListItem button onClick={toggleDropDown}>
                <ListItemText primary="Settings"/>
                {setNotification()}
            </ListItem>
            <Collapse in={dropDown} timeout="auto" unmountOnExit>
                <List className={classes.root}>
                    <ListItem button component={Link} to="/settings/general">
                        <ListItemText className={classes.nested} primary="General"/>
                        {setNotification()}
                    </ListItem>
                    <ListItem button component={Link} to="/settings/setup">
                        <ListItemText className={classes.nested} primary="Setup"/>
                    </ListItem>
                    <ListItem button component={Link} to="/settings/quartz">
                        <ListItemText className={classes.nested} primary="Background Jobs"/>
                    </ListItem>
                </List>
            </Collapse>
        </>
    )
}