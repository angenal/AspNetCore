var keys = { key: 'TmIhgugCGFpU7S3v', iv: 'jkE49230Tf093b42' };
var key = CryptoJS.enc.Utf8.parse(keys.key), iv = CryptoJS.enc.Utf8.parse(keys.iv);

const encryptWithJavascript = () => {
    $('#decrypt').val('');
    var encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse($('#encrypt').val()), key, {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });
    $('#decrypt').val(encrypted);

    //console.log('Encrypted :' + encrypted);
    //console.log('Key :' + encrypted.key);
    //console.log('Salt :' + encrypted.salt);
    //console.log('iv :' + encrypted.iv);
}

const decryptWithJavascript = () => {
    $('#encrypt').val('');
    var decrypted = CryptoJS.AES.decrypt($('#decrypt').val(), key, {
        keySize: 128 / 8,
        iv: iv,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
    });
    $('#encrypt').val(decrypted.toString(CryptoJS.enc.Utf8));

    //console.log('Decrypted : ' + decrypted);
    //console.log('utf8 = ' + decrypted.toString(CryptoJS.enc.Utf8));
}

const encryptWithCsharp = () => {
    $('#decrypt').val('');
    $.ajax({
        type: 'POST',
        url: '/api/Data/Encrypt',
        data: JSON.stringify({ text: $('#encrypt').val(), keys: keys }),
        contentType: 'application/json',
        success: (data) => {
            $('#decrypt').val(data.text);
        },
        error: (errMsg) => {
            console.log(errMsg);
        }
    });
}

const decryptWithCsharp = () => {
    $('#encrypt').val('');
    $.ajax({
        type: 'POST',
        url: '/api/Data/Decrypt',
        data: JSON.stringify({ text: $('#decrypt').val(), keys: keys }),
        contentType: 'application/json',
        success: (data) => {
            $('#encrypt').val(data.text);
        },
        error: (errMsg) => {
            console.log(errMsg);
        }
    });
}
