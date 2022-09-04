window.addEventListener('load', function () {
    const updateButton = this.document.querySelector('#invitations-update-btn').addEventListener('click', getCourseInvitations);

    getCourseInvitations();
});

function getCourseInvitations() {
    console.log('getCourseInvitations');
    postRequestWithoutResponseBody(
        'https://localhost:7284/api/Invitations/update/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => () => { })
        .catch(error => { console.error(error), alert('Invitations are currently unavailable') })

    getRequest(
        'https://localhost:7284/api/Invitations/list/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => (showInvitations(response)))
        .catch(error => { console.error(error), alert('Invitations are currently unavailable') })
}

function getRequest(url) {
    return fetch(url,
        {
            method: "GET"
        }
    ).then(response => response.json());
}

function postRequestWithoutResponseBody(url, data) {
    return fetch(url,
        {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: data
        }
    );
}

function showInvitations(json) {
    const invitationsPlace = document.querySelector('#invitationsPlace');
    invitationsPlace.innerHTML = '';
    var invitationRow = document.querySelector('.invitationRow').cloneNode(true);
    for (let i = 0; i < json.length; i++) {
        invitationRow.querySelector('.invitation-number').innerHTML = i + 1;
        invitationRow.querySelector('.invitation-email').innerHTML = json[i]['email'];
        invitationRow.querySelector('.invitation-role').innerHTML = json[i]['role'];
        var status = 'accepted';
        invitationRow.querySelector('.invitation-status').classList.add('text-success');
        if (!json[i]['isAccepted']) {
            status = 'not accepted';
            invitationRow.querySelector('.invitation-status').classList.add('text-danger');
        }
        invitationRow.querySelector('.invitation-status').innerHTML = status;
        invitationRow.querySelector('.invitation-last-update').innerHTML = json[i]['updateTime'];
        invitationRow.classList.remove('d-none');
        invitationsPlace.appendChild(invitationRow);
    }
}