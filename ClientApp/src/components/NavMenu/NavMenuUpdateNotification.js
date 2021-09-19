import React, {useEffect, useState} from "react";
import {Collapse, List, ListItem, ListItemText, makeStyles} from "@material-ui/core";
import {Link} from "react-router-dom";
import WarningIcon from '@material-ui/icons/Warning';

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
    const [updateAvailable, setUpdateAvailable] = useState(false);
    const classes = styles();

    useEffect(() => {
        getUpdates();
    }, [])

    useEffect(() => {
        props.hubConnection.on("updateFound", (message) => {
            setUpdateAvailable(true);
        });
    }, [])

    async function getUpdates() {
        const request = await fetch('update/internalCheck',
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
        
        if (request.ok) {
            var response = await request.json();
            setUpdateAvailable(response.updateAvailable);
        }
    }

    function toggleDropDown() {
        setDropDown(prevState => !prevState);
    }

    return (
        <>
            <ListItem button onClick={toggleDropDown}>
                <ListItemText primary="Settings"/>
                {updateAvailable ?
                    <WarningIcon color={"secondary"}/>
                    :
                    null
                }
            </ListItem>
            <Collapse in={dropDown} timeout="auto" unmountOnExit>
                <List className={classes.root}>
                    <ListItem button component={Link} to="/settings/general">
                        <ListItemText className={classes.nested} primary="General"/>
                        {updateAvailable ?
                            <WarningIcon color={"secondary"}/>
                            :
                            null
                        }
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