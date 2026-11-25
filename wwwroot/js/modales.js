// Abrir formulario en modal
$(document).on('click', '.btn-modal', function () {
    $('#modalContent').load($(this).data('url'), function () {
        $.validator.unobtrusive.parse('#modalContent form');
        bootstrap.Modal.getOrCreateInstance('#modalForm').show();
    });
});

// Enviar formulario por AJAX
$(document).on('submit', '#modalContent form', function (e) {
    e.preventDefault();
    var form = $(this);
    if (!form.valid()) return;
    $.post(this.action, form.serialize()).done(function (r) {
        if (r && r.ok === true) {
            location.reload();
        } else if (r && r.ok === false) {
            alert(r.mensaje || 'No se pudo completar la operación.');
        } else {
            $('#modalContent').html(r);
            $.validator.unobtrusive.parse('#modalContent form');
        }
    }).fail(function () {
        alert('Ocurrió un error al guardar.');
    });
});

// Eliminar (lógico)
$(document).on('click', '.btn-eliminar', function () {
    if (!$(this).data('sinconfirmar') && !confirm('¿Desea eliminar este registro?')) return;
    $.post($(this).data('url')).done(function (r) {
        if (r && r.ok === false) alert(r.mensaje || 'No se pudo completar.');
        else location.reload();
    }).fail(function () { alert('Ocurrió un error.'); });
});