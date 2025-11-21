using Microsoft.AspNetCore.Mvc;
using Proyecto.Models;
using Microsoft.AspNetCore.Identity;
using Proyecto.Data;
using Proyecto.shared;
using Microsoft.EntityFrameworkCore;
using Proyecto.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Proyecto.Controllers
{
    [Authorize(Roles = VCG.Role_Admin)]
    public class AdministradorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBContext _context;

        public AdministradorController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AppDBContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Administrador/Register
        public IActionResult Register()
        {
            NewRegisterTypeUserVM dataResponse = new NewRegisterTypeUserVM
            {
                cursos = _context.Cursos.ToList(),
                tutores = _context.Tutores.Include(t => t.user).ToList()
            };
            return View(dataResponse);
        }

        // POST: /Administrador/Register
        [HttpPost]
        public async Task<IActionResult> Register(NewRegisterTypeUserVM userVM)
        {
            if (!ModelState.IsValid)
            {
                // Recargar los datos necesarios para la vista antes de devolverla
                userVM.cursos = _context.Cursos.ToList();
                userVM.tutores = _context.Tutores.Include(t => t.user).ToList();
                return View(userVM);
            }
            string tipo = userVM.tipo ?? string.Empty;
            Console.WriteLine("======================================== tipo ========================================");
            Console.WriteLine(tipo);


            var result = await _userManager.CreateAsync(userVM.User, userVM.User.Dni);
            if (!result.Succeeded)
            {
                Console.WriteLine("======================================== error ========================================");
                // si hay errores los imprimimos
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
                ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
                // Recargar los datos necesarios para la vista
                userVM.cursos = _context.Cursos.ToList();
                userVM.tutores = _context.Tutores.Include(t => t.user).ToList();
                return View(userVM); // Pasar el modelo de vuelta a la vista
            }

            if (tipo == TypesRegister.Tutor)
            {
                await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Tutor);
                var tutor = new Tutor
                {
                    UserId = userVM.User.Id,
                    direccion = "" // Puedes agregar campo en el formulario si lo necesitas
                };
                _context.Tutores.Add(tutor);
                await _context.SaveChangesAsync();

                // Registrar hija si se ingresó
                if (!string.IsNullOrEmpty(userVM.estudiante.user.UserName))
                {
                    await _userManager.CreateAsync(userVM.estudiante.user, userVM.estudiante.user.Dni); // Contraseña temporal
                    var estudiante = new Estudiante
                    {
                        UserId = userVM.estudiante.user.Id,
                        TutorId = tutor.IdTutor,
                        Grado = userVM.estudiante.Grado
                    };
                    _context.Estudiantes.Add(estudiante);
                    await _context.SaveChangesAsync();
                }
            }
            else if (tipo == TypesRegister.Estudiante)
            {
                await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Estudiante);
                if (userVM.estudiante.TutorId <= 0 && !string.IsNullOrEmpty(userVM.tutor.user.UserName))
                {
                    await _userManager.CreateAsync(userVM.tutor.user, userVM.tutor.user.Dni); // Contraseña temporal
                    await _userManager.AddToRoleAsync(userVM.tutor.user, VCG.Role_Tutor);
                    Console.WriteLine("================================================================================");
                    Console.WriteLine($"user id: {userVM.tutor.user.Id}");
                    userVM.tutor.UserId = userVM.tutor.user.Id;
                    _context.Tutores.Add(userVM.tutor);
                    await _context.SaveChangesAsync();
                    userVM.estudiante.TutorId = userVM.tutor.IdTutor;
                }
                Console.WriteLine($"id final del tutor: {userVM.estudiante.TutorId}");
                userVM.estudiante.UserId = userVM.User.Id;

                _context.Estudiantes.Add(userVM.estudiante);
                await _context.SaveChangesAsync();
            }
            else if (tipo == TypesRegister.Docente)
            {
                await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Docente);
                if (!string.IsNullOrEmpty(userVM.curso.Nombre))
                {
                    _context.Cursos.Add(userVM.curso);
                    await _context.SaveChangesAsync();
                }
                else if (userVM.curso.IdCurso > 0)
                {
                    userVM.curso = _context.Cursos.FirstOrDefault(c => c.IdCurso == userVM.curso.IdCurso);
                }
                var docente = new Docente
                {
                    UserId = userVM.User.Id,
                    Curso = userVM.curso
                };
                _context.Docentes.Add(docente);
                await _context.SaveChangesAsync();
            }
            else if (tipo == TypesRegister.Administrador)
            {
                await _userManager.AddToRoleAsync(userVM.User, VCG.Role_Admin);
                // Puedes agregar lógica adicional si tienes tabla Administrador
            }

            return RedirectToAction("Dashboard");
        }

        // GET: /Administrador/RegisterCurso
        [HttpGet]
        public IActionResult RegisterCurso()
        {
            NewCursoVM cursoVM = new NewCursoVM()
            {
                Curso = new Curso(),
                DocenteId = 0,
                Docentes = _context.Docentes.Include(u => u.user).ToList(),
            };
            return View(cursoVM);
        }

        // POST: /Administrador/RegisterCurso
        [HttpPost]
        public async Task<IActionResult> RegisterCurso(NewCursoVM cursoVM)
        {
            if (!ModelState.IsValid)
            {
                // Repoblar datos necesarios para la vista
                cursoVM.Docentes = _context.Docentes.Include(u => u.user).ToList();
                return View(cursoVM);
            }
            _context.Cursos.Add(cursoVM.Curso);
            await _context.SaveChangesAsync();

            // Asignar docente si corresponde
            if (cursoVM.DocenteId > 0)
            {
                int docenteId = cursoVM.DocenteId;
                var docente = _context.Docentes.FirstOrDefault(d => d.IdDocente == docenteId);
                if (docente != null)
                {
                    docente.Curso = cursoVM.Curso;
                    _context.Docentes.Update(docente);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> DetalleCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.estudiante_Curso)
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.User = await _userManager.GetUserAsync(User);
            ViewBag.UsersCount = await _userManager.Users.CountAsync();
            ViewBag.AlumnosCount = await _context.Estudiantes.CountAsync();
            ViewBag.DocentesCount = await _context.Docentes.CountAsync();
            ViewBag.CursosCount = await _context.Cursos.CountAsync();
            return View();
        }

        public async Task<IActionResult> Cursos()
        {
            var curso = await _context.Cursos
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .Include(c => c.estudiante_Curso)
                .ToListAsync();
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.estudiante_Curso)
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
            {
                return Json(new { success = false, message = "Curso no encontrado." });
            }

            if (curso.estudiante_Curso?.Any() ?? false)
            {
                return Json(new { success = false, message = "No se puede eliminar el curso porque tiene estudiantes asignados." });
            }

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Curso eliminado correctamente." });
        }

        public async Task<IActionResult> ActualizarCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Docentes)
                .ThenInclude(d => d.user)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
            {
                return NotFound();
            }

            // Crear el ViewModel para la vista
            var cursoVM = new NewCursoVM
            {
                Curso = curso,
                // Cargar todos los docentes para el dropdown de asignación
                Docentes = await _context.Docentes
                    .Where(d => d.CursoId == null)
                    .Include(d => d.user)
                    .ToListAsync()
            };

            return View(cursoVM);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCurso(NewCursoVM cursoVM)
        {
            if (!ModelState.IsValid)
            {
                // Repoblar los docentes para el dropdown y devolver la vista
                cursoVM.Docentes = await _context.Docentes
                    .Where(d => d.CursoId == null)
                    .Include(d => d.user)
                    .ToListAsync();
                return View(cursoVM);
            }
            // Buscar el curso existente en la base de datos
            var cursoExist = await _context.Cursos.FindAsync(cursoVM.Curso.IdCurso);
            if (cursoExist == null)
            {
                return NotFound();
            }

            // Actualizar las propiedades del curso
            cursoExist.Nombre = cursoVM.Curso.Nombre;
            cursoExist.HorarioInicio = cursoVM.Curso.HorarioInicio;
            cursoExist.HorarioFin = cursoVM.Curso.HorarioFin;
            cursoExist.aula = cursoVM.Curso.aula;
            cursoExist.Grado = cursoVM.Curso.Grado;

            _context.Cursos.Update(cursoExist);

            if (cursoVM.DocenteId > 0)
            {
                var docente = await _context.Docentes.FindAsync(cursoVM.DocenteId);
                if (docente != null)
                {
                    docente.CursoId = cursoExist.IdCurso;
                    _context.Docentes.Update(docente);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Cursos");
        }


        public async Task<IActionResult> Usuarios()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> DetalleUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);

            UserDetailVM responseData = new UserDetailVM();

            responseData.UserId = user.Id;
            responseData.role = roles;

            if (roles.Contains(VCG.Role_Tutor))
            {
                responseData.tutor = await _context.Tutores.Include(t => t.user)
                                          .Include(t => t.Estudiantes)
                                          .ThenInclude(e => e.user)
                                          .FirstOrDefaultAsync(t => t.UserId == user.Id);

            }
            else if (roles.Contains(VCG.Role_Estudiante))
            {
                responseData.estudiante = await _context.Estudiantes
                                                  .Include(e => e.user)
                                               .Include(e => e.Tutor)
                                               .ThenInclude(t => t.user)
                                               .Include(e => e.Estudiante_Cursos)
                                               .ThenInclude(ec => ec.Curso)
                                               .ThenInclude(c => c.Docentes)
                                               .ThenInclude(d => d.user)
                                               .FirstOrDefaultAsync(e => e.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Docente))
            {
                responseData.docente = await _context.Docentes
                                            .Include(d => d.user)
                                            .Include(d => d.Curso)
                                            .ThenInclude(c => c.estudiante_Curso)
                                            .FirstOrDefaultAsync(d => d.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Admin))
            {
                responseData.Administrador = user;
            }

            return View(responseData);
        }

        public async Task<IActionResult> EliminarUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);

            UserDetailVM responseData = new UserDetailVM();

            responseData.UserId = user.Id;
            responseData.role = roles;

            if (roles.Contains(VCG.Role_Tutor))
            {
                responseData.tutor = await _context.Tutores.Include(t => t.user)
                                          .Include(t => t.Estudiantes)
                                          .ThenInclude(e => e.user)
                                          .FirstOrDefaultAsync(t => t.UserId == user.Id);

            }
            else if (roles.Contains(VCG.Role_Estudiante))
            {
                responseData.estudiante = await _context.Estudiantes
                                                  .Include(e => e.user)
                                               .Include(e => e.Tutor)
                                               .ThenInclude(t => t.user)
                                               .Include(e => e.Estudiante_Cursos)
                                               .ThenInclude(ec => ec.Curso)
                                               .ThenInclude(c => c.Docentes)
                                               .ThenInclude(d => d.user)
                                               .FirstOrDefaultAsync(e => e.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Docente))
            {
                responseData.docente = await _context.Docentes
                                            .Include(d => d.user)
                                            .Include(d => d.Curso)
                                            .ThenInclude(c => c.estudiante_Curso)
                                            .FirstOrDefaultAsync(d => d.UserId == user.Id);
            }
            else if (roles.Contains(VCG.Role_Admin))
            {
                responseData.Administrador = user;
            }

            return View(responseData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUser(UserDetailVM user)
        {
            if (!ModelState.IsValid)
            {
                return View("DetalleUsuario", user);
            }
            if (string.IsNullOrEmpty(user.UserId))
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(user.UserId);
            if (usuario == null)
            {
                return NotFound();
            }

            var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.UserId == user.UserId);
            if (docente != null)
            {
                _context.Docentes.Remove(docente);
            }

            var estudiante = await _context.Estudiantes.FirstOrDefaultAsync(e => e.UserId == user.UserId);
            if (estudiante != null)
            {
                _context.Estudiantes.Remove(estudiante);
            }

            var tutor = await _context.Tutores.FirstOrDefaultAsync(a => a.UserId == user.UserId);
            if (tutor != null)
            {
                _context.Tutores.Remove(tutor);
            }

            await _context.SaveChangesAsync();

            var resultado = await _userManager.DeleteAsync(usuario);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Usuarios");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("DetalleUsuario", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesvincularDocente([FromBody] DesvincularDocenteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Solicitud inválida." });
            }
            try
            {
                var docente = await _context.Docentes
                    .Include(d => d.user)
                    .FirstOrDefaultAsync(d => d.IdDocente == request.DocenteId);

                if (docente == null)
                {
                    return Json(new { success = false, message = "Docente no encontrado." });
                }

                if (docente.CursoId != request.CursoId)
                {
                    return Json(new { success = false, message = "El docente no está asignado a este curso." });
                }

                docente.CursoId = null;
                _context.Docentes.Update(docente);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Docente desvinculado correctamente.",
                    docenteNombre = $"{docente.user.UserName} {docente.user.Apellido}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor." });
            }
        }
    }
}
