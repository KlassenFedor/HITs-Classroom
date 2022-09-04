window.addEventListener('load', function () {
    const updateButton = this.document.querySelector('#invitations-update-btn').addEventListener('clic', getCourseInvitations);
});

function getCourseInvitations() {
    console.log('getCourseInvitations');
    getRequest(
        'https://localhost:7284/api/invitations/list/' + window.location.href.split('?')[1].split('=')[1]
    )
        .then(response => (showInvitations(response))
        .catch(error => { console.error(error), alert('invitations are currently unavailable') })
}

function getRequest(url) {
    return fetch(url,
        {
            method: "GET"
        }
    ).then(response => response.json());
}

function showInvitations(json) {
    const invitationsPlace = document.querySelector('#invitationsPlace');
    const invitationRow = document.querySelector('.invitationRow');
}