import React from "react";
import "../../../assets/styles/StreamSearch.css";
import {IconButton, makeStyles, MenuItem, SvgIcon} from "@material-ui/core";

const styles = makeStyles((theme) => ({
    menuIcons: {
        padding: 0,
        paddingRight: 12
    }
}));

export default function StreamerGetChat(stream) {
    const classes = styles();

    async function handleButtonClick() {
        const response = await fetch('chat/' + stream.id,
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (response.ok) {
            var jsonResponse = await response.json();
            const link = document.createElement('a');
            link.download = stream.id + ".txt";
            link.href = jsonResponse.url;
            document.body.append(link);
            link.click();
            document.body.removeChild(link);
        }
    }

    return (
        <div>
            <MenuItem onClick={handleButtonClick} disabled={!stream.downloaded}>
                <IconButton className={classes.menuIcons}>
                    <SvgIcon>
                        <path fill="currentColor"
                              d="M20,20H7A2,2 0 0,1 5,18V8.94L2.23,5.64C2.09,5.47 2,5.24 2,5A1,1 0 0,1 3,4H20A2,2 0 0,1 22,6V18A2,2 0 0,1 20,20M8.5,7A0.5,0.5 0 0,0 8,7.5V8.5A0.5,0.5 0 0,0 8.5,9H18.5A0.5,0.5 0 0,0 19,8.5V7.5A0.5,0.5 0 0,0 18.5,7H8.5M8.5,11A0.5,0.5 0 0,0 8,11.5V12.5A0.5,0.5 0 0,0 8.5,13H18.5A0.5,0.5 0 0,0 19,12.5V11.5A0.5,0.5 0 0,0 18.5,11H8.5M8.5,15A0.5,0.5 0 0,0 8,15.5V16.5A0.5,0.5 0 0,0 8.5,17H13.5A0.5,0.5 0 0,0 14,16.5V15.5A0.5,0.5 0 0,0 13.5,15H8.5Z"/>
                    </SvgIcon>
                </IconButton>
                Download Chat</MenuItem>
        </div>
    )
}
