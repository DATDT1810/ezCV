/* global $, alert, console */
$(document).ready(function () {
    'use strict';

    // === Hàm chung thêm nút xóa ===
    function addRemoveButton(targetSelector) {
        const removeBtn = $('<button type="button" class="btn btn-danger btn-sm remove-block mt-2"><i class="fa fa-trash"></i> Xóa mục này</button>');
        $(targetSelector).append(removeBtn);
    }

    // === Thêm Học vấn ===
    $('#add-edu').on('click', function () {
        const eduBlock = $(`
            <div class="edu-item mt-3">
                <div class="add-border"><span></span><h2>Học vấn mới</h2><span></span></div>
                <div class="new-edu">
                    <label>Ngành học:</label>
                    <input type="text" name="edu[]" class="form-control" placeholder="VD: Kỹ thuật phần mềm">
                    <label>Bằng cấp:</label>
                    <input type="text" name="edu[]" class="form-control" placeholder="VD: Cử nhân">
                    <label>Trường học:</label>
                    <input type="text" name="edu[]" class="form-control" placeholder="VD: Đại học ABC">
                    <div class="form-row">
                        <div class="col">
                            <label>Từ năm:</label>
                            <input type="month" name="edu[]" class="form-control">
                        </div>
                        <div class="col">
                            <label>Đến năm (tùy chọn = hiện tại):</label>
                            <input type="month" name="edu[]" class="form-control">
                        </div>
                    </div>
                </div>
            </div>
        `);
        addRemoveButton(eduBlock);
        $('.all-edus').append(eduBlock);
    });

    // === Thêm Kinh nghiệm ===
    $('#add-exp').on('click', function () {
        const expBlock = $(`
            <div class="exp-item mt-3">
                <div class="add-border"><span></span><h2>Kinh nghiệm mới</h2><span></span></div>
                <div class="new-exp">
                    <label>Tiêu đề:</label>
                    <input type="text" name="exp[]" class="form-control" placeholder="VD: Lập trình viên web">
                    <label>Công ty:</label>
                    <input type="text" name="exp[]" class="form-control" placeholder="VD: Công ty ABC">
                    <div class="form-row">
                        <div class="col">
                            <label>Từ năm:</label>
                            <input type="month" name="exp[]" class="form-control">
                        </div>
                        <div class="col">
                            <label>Đến năm (tùy chọn = hiện tại):</label>
                            <input type="month" name="exp[]" class="form-control">
                        </div>
                    </div>
                    <label>Mô tả (tùy chọn):</label>
                    <textarea name="exp[]" class="form-control" placeholder="Mô tả công việc hoặc thành tích của bạn..."></textarea>
                </div>
            </div>
        `);
        addRemoveButton(expBlock);
        $('.all-exps').append(expBlock);
    });

    // === Thêm Kỹ năng ===
    $('.add-skills').on('click', function () {
        const skillBlock = $(`
            <div class="skill-item mt-3">
                <div class="add-border"><span></span><h2>Kỹ năng mới</h2><span></span></div>
                <div class="new-skills">
                    <label>Kỹ năng:</label>
                    <input type="text" name="skills[]" class="form-control" placeholder="VD: HTML, C#, Photoshop...">
                    <label>Trình độ:</label>
                    <input type="text" name="skills[]" class="form-control" placeholder="VD: Cơ bản, Trung bình, Thành thạo...">
                </div>
            </div>
        `);
        addRemoveButton(skillBlock);
        $('.all-skills').append(skillBlock);
    });

    // === Thêm Mạng xã hội ===
    $('.add-socials').on('click', function () {
        const socialBlock = $(`
            <div class="social-item mt-3">
                <div class="add-border"><span></span><h2>Mạng xã hội mới</h2><span></span></div>
                <div class="new-socials">
                    <label>Tên mạng xã hội:</label>
                    <input type="text" name="social" class="form-control" placeholder="VD: Facebook, LinkedIn...">
                    <label>Liên kết mạng xã hội:</label>
                    <input type="text" name="socials[]" class="form-control" placeholder="VD: https://facebook.com/tenban">
                    <label>Biểu tượng (16px × 16px):</label>
                    <input type="file" name="socials[]" class="form-control" />
                </div>
            </div>
        `);
        addRemoveButton(socialBlock);
        $('.all-socials').append(socialBlock);
    });

    // === Thêm Sở thích ===
    $('.add-hoppies').on('click', function () {
        const hobbyBlock = $(`
            <div class="hoppy-item mt-3">
                <div class="add-border"><span></span><h2>Sở thích mới</h2><span></span></div>
                <div class="new-hoppies">
                    <label>Sở thích:</label>
                    <input type="text" name="hoppies[]" class="form-control" placeholder="VD: Đọc sách, Du lịch, Chơi thể thao...">
                </div>
            </div>
        `);
        addRemoveButton(hobbyBlock);
        $('.all-hoppies').append(hobbyBlock);
    });

    // === Xóa khối đã thêm ===
    $(document).on('click', '.remove-block', function () {
        if (confirm('Bạn có chắc chắn muốn xóa mục này không?')) {
            $(this).closest('.edu-item, .exp-item, .skill-item, .social-item, .hoppy-item').slideUp(300, function () {
                $(this).remove();
            });
        }
    });
});
